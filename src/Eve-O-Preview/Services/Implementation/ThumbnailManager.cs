using System;
using System.Collections.Generic;
using EveOPreview.Configuration;
using EveOPreview.Presenters;
using EveOPreview.Services.Interop;
using EveOPreview.UI.Hotkeys;
using EveOPreview.View;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace EveOPreview.Services
{
	sealed class ThumbnailManager : IThumbnailManager
	{
		#region Private constants
		private const int WINDOW_POSITION_THRESHOLD_LOW = -10_000;
		private const int WINDOW_POSITION_THRESHOLD_HIGH = 31_000;
		private const int WINDOW_SIZE_THRESHOLD = 10;
		private const int FORCED_REFRESH_CYCLE_THRESHOLD = 2;
		private const int DEFAULT_LOCATION_CHANGE_NOTIFICATION_DELAY = 2;

		private const string DEFAULT_CLIENT_TITLE = "EVE";
		#endregion

		#region Private fields
		private readonly IConfigurationStorage _configurationStorage;
		private IMainFormPresenter _viewNotifier;
		private readonly IProcessMonitor _processMonitor;
		private readonly IWindowManager _windowManager;
		private readonly IThumbnailConfiguration _configuration;
		private readonly DispatcherTimer _thumbnailUpdateTimer;
		private readonly IThumbnailViewFactory _thumbnailViewFactory;
		private readonly Dictionary<IntPtr, IThumbnailView> _thumbnailViews;

		private (IntPtr Handle, string Title) _activeClient;
		private IntPtr _externalApplication;

		private readonly object _locationChangeNotificationSyncRoot;
		private (IntPtr Handle, string Title, string ActiveClient, Point Location, int Delay) _enqueuedLocationChangeNotification;

		private bool _ignoreViewEvents;
		private bool _isHoverEffectActive;

		private int _refreshCycleCount;
		private int _hideThumbnailsDelay;
		private bool _isThumbnailUpdateInProgress;
		private bool _isRefreshThumbnailsInProgress;
		private bool _refreshThumbnailsPending;

		private readonly GlobalMouseInputHandler _globalMouseInputHandler;
		private readonly List<HotkeyHandler> _primaryCycleHotkeyHandlers = new List<HotkeyHandler>();
		private readonly List<string> _primaryCycleMouseBindings = new List<string>();
		private readonly List<HotkeyHandler> _minimizeAllHotkeyHandlers = new List<HotkeyHandler>();
		private readonly List<string> _minimizeAllMouseBindings = new List<string>();
		private readonly List<HotkeyHandler> _showAllPreviewsHotkeyHandlers = new List<HotkeyHandler>();
		private readonly List<string> _showAllPreviewsMouseBindings = new List<string>();

		// Runtime-only "show every client as a preview grid" overview mode. Not persisted.
		private bool _previewOverviewActive;
		#endregion

		public ThumbnailManager(IConfigurationStorage configurationStorage, IThumbnailConfiguration configuration, IProcessMonitor processMonitor, IWindowManager windowManager, IThumbnailViewFactory factory)
		{
			this._configurationStorage = configurationStorage;
			this._processMonitor = processMonitor;
			this._windowManager = windowManager;
			this._configuration = configuration;
			this._thumbnailViewFactory = factory;

			this._activeClient = (IntPtr.Zero, ThumbnailManager.DEFAULT_CLIENT_TITLE);

			this.EnableViewEvents();
			this._isHoverEffectActive = false;

			this._refreshCycleCount = 0;
			this._locationChangeNotificationSyncRoot = new object();
			this._enqueuedLocationChangeNotification = (IntPtr.Zero, null, null, Point.Empty, -1);

			this._thumbnailViews = new Dictionary<IntPtr, IThumbnailView>();

			//  DispatcherTimer setup
			this._thumbnailUpdateTimer = new DispatcherTimer();
			this._thumbnailUpdateTimer.Tick += ThumbnailUpdateTimerTick;
			this._thumbnailUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, configuration.ThumbnailRefreshPeriod);

			this._hideThumbnailsDelay = this._configuration.HideThumbnailsDelay;
			this._globalMouseInputHandler = new GlobalMouseInputHandler();
		}

		public void AttachPresenter(IMainFormPresenter presenter)
		{
			this._viewNotifier = presenter;
		}

		public void UpdateActionBindings()
		{
			void updateCore()
			{
				this.ClearActionBindings();
				this.RegisterPrimaryCycleBindingList(this._configuration.CycleGroup1ForwardHotkeys, true);
				this.RegisterMinimizeAllBindingList(this._configuration.MinimizeAllClientsHotkeys);
				this.RegisterShowAllPreviewsBindingList(this._configuration.ShowAllPreviewsHotkeys);
			}

			if (Application.OpenForms.Count > 0 && Application.OpenForms[0].InvokeRequired)
			{
				Application.OpenForms[0].BeginInvoke((Action)updateCore);
			}
			else
			{
				updateCore();
			}
		}

		private void ClearActionBindings()
		{
			foreach (HotkeyHandler handler in this._primaryCycleHotkeyHandlers)
			{
				handler.Dispose();
			}

			this._primaryCycleHotkeyHandlers.Clear();

			foreach (HotkeyHandler handler in this._minimizeAllHotkeyHandlers)
			{
				handler.Dispose();
			}

			this._minimizeAllHotkeyHandlers.Clear();

			foreach (string binding in this._primaryCycleMouseBindings)
			{
				this._globalMouseInputHandler.Unregister(binding);
			}

			this._primaryCycleMouseBindings.Clear();

			foreach (string binding in this._minimizeAllMouseBindings)
			{
				this._globalMouseInputHandler.Unregister(binding);
			}

			this._minimizeAllMouseBindings.Clear();

			foreach (HotkeyHandler handler in this._showAllPreviewsHotkeyHandlers)
			{
				handler.Dispose();
			}

			this._showAllPreviewsHotkeyHandlers.Clear();

			foreach (string binding in this._showAllPreviewsMouseBindings)
			{
				this._globalMouseInputHandler.Unregister(binding);
			}

			this._showAllPreviewsMouseBindings.Clear();
		}

		private void RegisterPrimaryCycleBindingList(List<string> bindings, bool isForwards)
		{
			if (bindings == null)
			{
				return;
			}

			foreach (string binding in bindings)
			{
				if (string.IsNullOrWhiteSpace(binding))
				{
					continue;
				}

				string trimmedBinding = binding.Trim();
				if (InputBindingHelper.GetKind(trimmedBinding) == InputBindingKind.Keyboard)
				{
					Keys key = this._configuration.StringToKey(trimmedBinding);
					if (key == Keys.None)
					{
						continue;
					}

					HotkeyHandler handler = new HotkeyHandler(this.GetHotkeyTarget(), key);
					handler.Pressed += (object sender, HandledEventArgs eventArgs) =>
					{
						this.SyncActiveClientFromForeground();
						this.CycleNextClientByHandle(isForwards);
						eventArgs.Handled = true;
					};

					if (handler.Register())
					{
						this._primaryCycleHotkeyHandlers.Add(handler);
					}
				}
				else
				{
					this._globalMouseInputHandler.Register(trimmedBinding, () => this.InvokeOnUiThread(() => this.InvokePrimaryMouseCycle(isForwards)));
					this._primaryCycleMouseBindings.Add(trimmedBinding);
				}
			}
		}

		private void RegisterMinimizeAllBindingList(List<string> bindings)
		{
			if (bindings == null)
			{
				return;
			}

			foreach (string binding in bindings)
			{
				if (string.IsNullOrWhiteSpace(binding))
				{
					continue;
				}

				string trimmedBinding = binding.Trim();
				if (InputBindingHelper.GetKind(trimmedBinding) == InputBindingKind.Keyboard)
				{
					Keys key = this._configuration.StringToKey(trimmedBinding);
					if (key == Keys.None)
					{
						continue;
					}

					HotkeyHandler handler = new HotkeyHandler(this.GetHotkeyTarget(), key);
					handler.Pressed += (object sender, HandledEventArgs eventArgs) =>
					{
						this.MinimizeAllClients();
						eventArgs.Handled = true;
					};

					if (handler.Register())
					{
						this._minimizeAllHotkeyHandlers.Add(handler);
					}
				}
				else
				{
					this._globalMouseInputHandler.Register(trimmedBinding, () => this.InvokeOnUiThread(this.MinimizeAllClients));
					this._minimizeAllMouseBindings.Add(trimmedBinding);
				}
			}
		}

		private void RegisterShowAllPreviewsBindingList(List<string> bindings)
		{
			if (bindings == null)
			{
				return;
			}

			foreach (string binding in bindings)
			{
				if (string.IsNullOrWhiteSpace(binding))
				{
					continue;
				}

				string trimmedBinding = binding.Trim();
				if (InputBindingHelper.GetKind(trimmedBinding) == InputBindingKind.Keyboard)
				{
					Keys key = this._configuration.StringToKey(trimmedBinding);
					if (key == Keys.None)
					{
						continue;
					}

					HotkeyHandler handler = new HotkeyHandler(this.GetHotkeyTarget(), key);
					handler.Pressed += (object sender, HandledEventArgs eventArgs) =>
					{
						this.ToggleAllPreviews();
						eventArgs.Handled = true;
					};

					if (handler.Register())
					{
						this._showAllPreviewsHotkeyHandlers.Add(handler);
					}
				}
				else
				{
					this._globalMouseInputHandler.Register(trimmedBinding, () => this.InvokeOnUiThread(this.ToggleAllPreviews));
					this._showAllPreviewsMouseBindings.Add(trimmedBinding);
				}
			}
		}

		private void InvokePrimaryMouseCycle(bool isForwards)
		{
			if (this._thumbnailViews.Count == 0)
			{
				return;
			}

			this.SyncActiveClientFromForeground();
			this.CycleNextClientByHandle(isForwards);
		}

		private void InvokeOnUiThread(Action action)
		{
			if (Application.OpenForms.Count == 0)
			{
				action();
				return;
			}

			Form mainForm = Application.OpenForms[0];
			if (mainForm.InvokeRequired)
			{
				mainForm.BeginInvoke(action);
			}
			else
			{
				action();
			}
		}

		private IntPtr GetHotkeyTarget()
		{
			return Application.OpenForms.Count > 0 ? Application.OpenForms[0].Handle : IntPtr.Zero;
		}

		private void SyncActiveClientFromForeground()
		{
			IntPtr foregroundWindowHandle = this._windowManager.GetForegroundWindowHandle();
			if (foregroundWindowHandle == IntPtr.Zero)
			{
				return;
			}

			if (this._thumbnailViews.TryGetValue(foregroundWindowHandle, out IThumbnailView foregroundView))
			{
				this._activeClient = (foregroundWindowHandle, foregroundView.Title);
				return;
			}

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				if (entry.Value.IsKnownHandle(foregroundWindowHandle))
				{
					this._activeClient = (entry.Key, entry.Value.Title);
					return;
				}
			}
		}

		public IThumbnailView GetClientByTitle(string title)
		{
			return _thumbnailViews.FirstOrDefault(x => x.Value.Title == title).Value;
		}

		public IThumbnailView GetClientByPointer(IntPtr ptr)
		{
			return _thumbnailViews.FirstOrDefault(x => x.Key == ptr).Value;
		}

		public IThumbnailView GetActiveClient()
		{
			return GetClientByPointer(this._activeClient.Handle);
		}

		public void SetActive(KeyValuePair<IntPtr, IThumbnailView> newClient)
		{
			this.GetActiveClient()?.ClearBorder();
			bool activated = this._windowManager.ActivateWindow(newClient.Key, this._configuration.WindowsAnimationStyle);
			if (!activated)
			{
				return;
			}

			this.SwitchActiveClient(newClient.Key, newClient.Value.Title);

			newClient.Value.SetHighlight();
			newClient.Value.Refresh(true);
		}

		public void MinimizeAllClients()
		{
			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews.Reverse())
			{
				if (!this.IsCycleEligible(entry.Value))
				{
					continue;
				}

				this._windowManager.MinimizeWindow(entry.Value.Id, this._configuration.WindowsAnimationStyle, false);
			}
		}

		private void CycleNextClientByHandle(bool isForwards)
		{
			List<KeyValuePair<IntPtr, IThumbnailView>> clients = this._thumbnailViews
				.Where(entry => this.IsCycleEligible(entry.Value))
				.OrderBy(entry => entry.Value.Id.ToInt64())
				.ToList();

			if (clients.Count == 0)
			{
				return;
			}

			if (clients.Count == 1)
			{
				this.SetActive(clients[0]);
				return;
			}

			int currentIndex = clients.FindIndex(entry =>
				entry.Key == this._activeClient.Handle
				|| entry.Value.Id == this._activeClient.Handle);

			if (currentIndex < 0)
			{
				currentIndex = 0;
			}

			int nextIndex = isForwards
				? (currentIndex + 1) % clients.Count
				: (currentIndex - 1 + clients.Count) % clients.Count;

			this.SetActive(clients[nextIndex]);
		}

		// Toggle a temporary "see every client at once" preview grid. Turning it on
		// restores any minimized clients (so DWM can render their thumbnails) and
		// tiles all previews across the primary monitor. Turning it off puts the
		// clients back to the normal cycle state (inactive, non-priority clients
		// minimized again).
		public void ToggleAllPreviews()
		{
			if (!this._previewOverviewActive)
			{
				this._previewOverviewActive = true;
				this.RestoreAllClientsForPreview();
			}
			else
			{
				this.ExitPreviewOverview();
			}

			this.RequestRefreshThumbnails();
		}

		private void ExitPreviewOverview()
		{
			this._previewOverviewActive = false;
			this.RestoreConfiguredSizeLimits();
			this.MinimizeInactiveClientsAfterOverview();
		}

		private void RestoreConfiguredSizeLimits()
		{
			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				entry.Value.SetSizeLimitations(this._configuration.ThumbnailMinimumSize, this._configuration.ThumbnailMaximumSize);
			}
		}

		private void RestoreAllClientsForPreview()
		{
			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				if (entry.Value.Id == IntPtr.Zero)
				{
					continue;
				}

				this._windowManager.RestoreWindow(entry.Value.Id);
			}
		}

		private void MinimizeInactiveClientsAfterOverview()
		{
			if (!this._configuration.MinimizeInactiveClients)
			{
				return;
			}

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				if (!this.IsCycleEligible(entry.Value))
				{
					continue;
				}

				// Keep the active client up, and never minimize a priority client.
				if (entry.Key == this._activeClient.Handle
					|| this._configuration.IsPriorityClient(entry.Value.Title))
				{
					continue;
				}

				this._windowManager.MinimizeWindow(entry.Value.Id, this._configuration.WindowsAnimationStyle, false);
			}
		}

		private void RefreshOverviewLayout(bool forceRefresh)
		{
			List<KeyValuePair<IntPtr, IThumbnailView>> views = this._thumbnailViews
				.Where(entry => entry.Value.Id != IntPtr.Zero)
				.OrderBy(entry => entry.Value.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
				.ThenBy(entry => entry.Key.ToInt64())
				.ToList();

			if (views.Count == 0)
			{
				return;
			}

			const int margin = 8;
			Rectangle area = this.GetOverviewScreenArea();
			List<Rectangle> cells = PreviewGridLayout.ComputeCells(views.Count, area, margin);

			for (int index = 0; index < views.Count; index++)
			{
				IThumbnailView view = views[index].Value;
				Rectangle cell = cells[index];

				Size cellSize = this.FitPreviewToCell(view.Id, cell.Width, cell.Height);

				// Center the aspect-fitted preview within its cell.
				int x = cell.X + ((cell.Width - cellSize.Width) / 2);
				int y = cell.Y + ((cell.Height - cellSize.Height) / 2);

				// Overview previews may be much larger than the normal thumbnail size
				// cap, so widen the size limits while the grid is shown.
				view.SetSizeLimitations(new Size(1, 1), new Size(area.Width, area.Height));
				view.ThumbnailSize = cellSize;
				view.ThumbnailLocation = new Point(x, y);
				view.SetOpacity(1.0);
				view.SetTopMost(true);
				view.IsOverlayEnabled = this._configuration.ShowThumbnailOverlays;
				view.SetHighlight(
					this._configuration.EnableActiveClientHighlight && (view.Id == this._activeClient.Handle),
					this._configuration.ActiveClientHighlightThickness);

				if (!view.IsActive)
				{
					view.Show();
				}
				else
				{
					view.Refresh(forceRefresh);
				}
			}
		}

		// The overview grid is shown on the second monitor when one is present,
		// falling back to the primary monitor for single-display setups.
		private Rectangle GetOverviewScreenArea()
		{
			foreach (Screen screen in Screen.AllScreens)
			{
				if (!screen.Primary)
				{
					return screen.WorkingArea;
				}
			}

			return Screen.PrimaryScreen.WorkingArea;
		}

		// Fit a preview into a grid cell, preserving the client's aspect ratio.
		private Size FitPreviewToCell(IntPtr handle, int availableWidth, int availableHeight)
		{
			if (availableWidth < 1)
			{
				availableWidth = 1;
			}

			if (availableHeight < 1)
			{
				availableHeight = 1;
			}

			Size clientSize = this._windowManager.GetClientAreaSize(handle);
			double aspect = (clientSize.Width > 0 && clientSize.Height > 0)
				? (double)clientSize.Width / clientSize.Height
				: 16.0 / 9.0;

			int width = availableWidth;
			int height = (int)Math.Round(width / aspect);

			if (height > availableHeight)
			{
				height = availableHeight;
				width = (int)Math.Round(height * aspect);
			}

			return new Size(Math.Max(width, 1), Math.Max(height, 1));
		}

		public void Start()
		{
			this._thumbnailUpdateTimer.Start();
			this.RequestRefreshThumbnails();
			this.UpdateActionBindings();
		}

		public void Stop()
		{
			this._previewOverviewActive = false;
			this._thumbnailUpdateTimer.Stop();
			this.ClearActionBindings();
			this._globalMouseInputHandler.Clear();
		}

		private async void ThumbnailUpdateTimerTick(object sender, EventArgs e)
		{
			if (this._isThumbnailUpdateInProgress)
			{
				return;
			}

			this._isThumbnailUpdateInProgress = true;
			try
			{
				await this.UpdateThumbnailsList();
				this.RequestRefreshThumbnails();
			}
			finally
			{
				this._isThumbnailUpdateInProgress = false;
			}
		}

		private async Task UpdateThumbnailsList()
		{
			this._processMonitor.GetUpdatedProcesses(out ICollection<IProcessInfo> addedProcesses, out ICollection<IProcessInfo> updatedProcesses, out ICollection<IProcessInfo> removedProcesses);

			List<string> viewsAdded = new List<string>();
			List<string> viewsRemoved = new List<string>();

			foreach (IProcessInfo process in addedProcesses)
			{
				Size configuredSize = this._configuration.PerClientThumbnailSize.TryGetValue(process.Title, out Size perClientSize)
					? perClientSize
					: this._configuration.ThumbnailSize;
				Size initialSize = this.GetAspectMatchedThumbnailSize(process.Handle, configuredSize);

				IThumbnailView view = this._thumbnailViewFactory.Create(process.Handle, process.Title, initialSize);
				view.IsOverlayEnabled = this._configuration.ShowThumbnailOverlays;
				view.IsExcludedFromCycleGroup = false;
				view.SetFrames(this._configuration.ShowThumbnailFrames);
				// Max/Min size limitations should be set AFTER the frames are disabled
				// Otherwise thumbnail window will be unnecessary resized
				view.SetSizeLimitations(this._configuration.ThumbnailMinimumSize, this._configuration.ThumbnailMaximumSize);
				view.SetTopMost(this._configuration.ShowThumbnailsAlwaysOnTop);
				view.SetClickThrough(this.IsThumbnailClickThroughEnabled());

				view.ThumbnailLocation = this.IsManageableThumbnail(view)
											? this._configuration.GetThumbnailLocation(view.Title, this._activeClient.Title, view.ThumbnailLocation)
											: this._configuration.LoginThumbnailLocation;

				this._thumbnailViews.Add(view.Id, view);

				view.ThumbnailResized = this.ThumbnailViewResized;
				view.ThumbnailMoved = this.ThumbnailViewMoved;
				view.ThumbnailFocused = this.ThumbnailViewFocused;
				view.ThumbnailLostFocus = this.ThumbnailViewLostFocus;
				view.ThumbnailActivated = this.ThumbnailActivated;
				view.ThumbnailDeactivated = this.ThumbnailDeactivated;

				view.ThumbnailToggleCycleGroup = this.ThumbnailToggleCycleGroup;

				view.RegisterHotkey(this._configuration.GetClientHotkey(view.Title));

				this.ApplyClientLayout(view);
				this.ApplyCaptionBar(view);

				if (!this._configuration.ShowThumbnailPreviews)
				{
					view.Hide();
				}

				// TODO Add extension filter here later
				if (view.Title != ThumbnailManager.DEFAULT_CLIENT_TITLE)
				{
					viewsAdded.Add(view.Title);
				}
			}

			foreach (IProcessInfo process in updatedProcesses)
			{
				this._thumbnailViews.TryGetValue(process.Handle, out IThumbnailView view);

				if (view == null)
				{
					// Something went terribly wrong
					continue;
				}

				if (process.Title != view.Title) // update thumbnail title
				{
					viewsRemoved.Add(view.Title);
					view.Title = process.Title;
					viewsAdded.Add(view.Title);

					view.RegisterHotkey(this._configuration.GetClientHotkey(process.Title));

					this.ApplyClientLayout(view);
					this.ApplyCaptionBar(view);
				}
			}

			foreach (IProcessInfo process in removedProcesses)
			{
				IThumbnailView view = this._thumbnailViews[process.Handle];

				this._thumbnailViews.Remove(view.Id);
				if (view.Title != ThumbnailManager.DEFAULT_CLIENT_TITLE)
				{
					viewsRemoved.Add(view.Title);
				}

				view.UnregisterHotkey();

				view.ThumbnailResized = null;
				view.ThumbnailMoved = null;
				view.ThumbnailFocused = null;
				view.ThumbnailLostFocus = null;
				view.ThumbnailActivated = null;
				view.ThumbnailToggleCycleGroup = null;

				view.Close();
			}

			if ((viewsAdded.Count > 0) || (viewsRemoved.Count > 0))
			{
				if (viewsAdded.Count > 0)
				{
					this._viewNotifier?.AddThumbnails(viewsAdded);
				}

				if (viewsRemoved.Count > 0)
				{
					this._viewNotifier?.RemoveThumbnails(viewsRemoved);
				}
			}
		}

		public void UpdateThumbnailVisibility()
		{
			this.RequestRefreshThumbnails();
		}

		private void RequestRefreshThumbnails()
		{
			if (this._isRefreshThumbnailsInProgress)
			{
				this._refreshThumbnailsPending = true;
				return;
			}

			this.RefreshThumbnails();
		}

		private void RefreshThumbnails()
		{
			if (this._isRefreshThumbnailsInProgress)
			{
				this._refreshThumbnailsPending = true;
				return;
			}

			this._isRefreshThumbnailsInProgress = true;
			try
			{
				this.RefreshThumbnailsCore();
			}
			finally
			{
				this._isRefreshThumbnailsInProgress = false;

				if (this._refreshThumbnailsPending)
				{
					this._refreshThumbnailsPending = false;
					this.RefreshThumbnails();
				}
			}
		}

		private void RefreshThumbnailsCore()
		{
			// TODO Split this method
			IntPtr foregroundWindowHandle = this._windowManager.GetForegroundWindowHandle();

			// The foreground window can be NULL in certain circumstances, such as when a window is losing activation.
			// It is safer to just skip this refresh round than to do something while the system state is undefined
			if (foregroundWindowHandle == IntPtr.Zero)
			{
				return;
			}

			string foregroundWindowTitle = null;

			// Check if the foreground window handle is one of the known handles for client windows or their thumbnails
			bool isClientWindow = this.IsClientWindowActive(foregroundWindowHandle);
			bool isMainWindowActive = this.IsMainWindowActive(foregroundWindowHandle);

			if (foregroundWindowHandle == this._activeClient.Handle)
			{
				foregroundWindowTitle = this._activeClient.Title;
			}
			else if (this._thumbnailViews.TryGetValue(foregroundWindowHandle, out IThumbnailView foregroundView))
			{
				// This code will work only on Alt+Tab switch between clients
				foregroundWindowTitle = foregroundView.Title;
			}
			else if (!isClientWindow)
			{
				this._externalApplication = foregroundWindowHandle;
			}

			// No need to minimize EVE clients when switching out to non-EVE window (like thumbnail)
			if (!string.IsNullOrEmpty(foregroundWindowTitle))
			{
				this.SwitchActiveClient(foregroundWindowHandle, foregroundWindowTitle);
			}

			bool hideAllThumbnails = this._configuration.HideThumbnailsOnLostFocus && !(isClientWindow || isMainWindowActive);

			// Wait for some time before hiding all previews
			if (hideAllThumbnails)
			{
				this._hideThumbnailsDelay--;
				if (this._hideThumbnailsDelay > 0)
				{
					hideAllThumbnails = false; // Postpone the 'hide all' operation
				}
				else
				{
					this._hideThumbnailsDelay = 0; // Stop the counter
				}
			}
			else
			{
				this._hideThumbnailsDelay = this._configuration.HideThumbnailsDelay; // Reset the counter
			}

			this._refreshCycleCount++;

			bool forceRefresh;
			if (this._refreshCycleCount >= ThumbnailManager.FORCED_REFRESH_CYCLE_THRESHOLD)
			{
				this._refreshCycleCount = 0;
				forceRefresh = true;
			}
			else
			{
				forceRefresh = false;
			}

			this.DisableViewEvents();

			// Preview overview mode: tile every client as a grid and skip the normal
			// per-client layout / hide logic entirely.
			if (this._previewOverviewActive)
			{
				this.RefreshOverviewLayout(forceRefresh);
				this.EnableViewEvents();
				return;
			}

			// Snap thumbnail
			// No need to update Thumbnails while one of them is highlighted
			if ((!this._isHoverEffectActive) && this.TryDequeueLocationChange(out var locationChange))
			{
				if ((locationChange.ActiveClient == this._activeClient.Title) && this._thumbnailViews.TryGetValue(locationChange.Handle, out var view))
				{
					this.SnapThumbnailView(view);

					this.RaiseThumbnailLocationUpdatedNotification(view.Title);
				}
				else
				{
					this.RaiseThumbnailLocationUpdatedNotification(locationChange.Title);
				}
			}

			// Hide, show, resize and move - update ZoomAnchor setting
			if (!this._configuration.ShowThumbnailPreviews)
			{
				foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
				{
					entry.Value.Hide();
				}

				this.EnableViewEvents();
				return;
			}

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				IThumbnailView view = entry.Value;
				// update ZoomAnchor regardless
				view.ClientZoomAnchor = this._configuration.GetZoomAnchor(view.Title, this._configuration.ThumbnailZoomAnchor);


				if (hideAllThumbnails || this._configuration.IsThumbnailDisabled(view.Title))
				{
					if (view.IsActive)
					{
						view.Hide();
					}
					continue;
				}

				if (this._configuration.HideActiveClientThumbnail && (view.Id == this._activeClient.Handle))
				{
					if (view.IsActive)
					{
						view.Hide();
					}
					continue;
				}

				if (this._configuration.HideLoginClientThumbnail && (view.Title == DEFAULT_CLIENT_TITLE ))
				{
					if (view.IsActive)
					{
						view.Hide();
					}
					continue;
				}

				// No need to update Thumbnails while one of them is highlighted
				if (!this._isHoverEffectActive)
				{
					// Do not even move thumbnails with default caption
					if (this.IsManageableThumbnail(view))
					{
						view.ThumbnailLocation = this._configuration.GetThumbnailLocation(view.Title, this._activeClient.Title, view.ThumbnailLocation);
						view.ThumbnailSize = this._configuration.GetThumbnailSize(view.Title, this._activeClient.Title, view.ThumbnailSize);
					}

					view.SetOpacity(this._configuration.ThumbnailOpacity);
					view.SetTopMost(this._configuration.ShowThumbnailsAlwaysOnTop);
				}

				view.IsOverlayEnabled = this._configuration.ShowThumbnailOverlays;

				view.SetHighlight(
					this._configuration.EnableActiveClientHighlight && (view.Id == this._activeClient.Handle), 
					this._configuration.ActiveClientHighlightThickness);

				if (!view.IsActive)
				{
					view.Show();
				}
				else
				{
					view.Refresh(forceRefresh);
				}
			}

			this.EnableViewEvents();
		}

		public void UpdateThumbnailsSize()
		{
			this.SetThumbnailsSize(this._configuration.ThumbnailSize);
		}
		public void UpdateCycleGroupIndicator()
		{
			this.SetCycleGroupIndicator(this._configuration.CycleGroupIndicatorAnchor);
		}

		private void SetCycleGroupIndicator(ZoomAnchor anchor)
		{
			this.DisableViewEvents();

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				entry.Value.SetCycleGroupIndicator(entry.Value.IsExcludedFromCycleGroup, anchor);
				entry.Value.Refresh(false);
			}

			this.EnableViewEvents();
		}

		private void SetThumbnailsSize(Size size)
		{
			this.DisableViewEvents();

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				entry.Value.ThumbnailSize = size;
				entry.Value.Refresh(false);
			}

			this.EnableViewEvents();
		}

		public void UpdateThumbnailFrames()
		{
			this.DisableViewEvents();

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				entry.Value.SetFrames(this._configuration.ShowThumbnailFrames);
				ApplyCaptionBar(entry.Value);
				entry.Value.SetPreventPreviews();
			}

			this.EnableViewEvents();
		}

		public void UpdateThumbnailClickThrough()
		{
			bool enabled = this.IsThumbnailClickThroughEnabled();

			this.DisableViewEvents();

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				entry.Value.SetClickThrough(enabled);
			}

			this.EnableViewEvents();
		}

		private bool IsThumbnailClickThroughEnabled()
		{
			return this._configuration.LockThumbnailLocation && this._configuration.ThumbnailClickThrough;
		}

		private void EnableViewEvents()
		{
			this._ignoreViewEvents = false;
		}

		private void DisableViewEvents()
		{
			this._ignoreViewEvents = true;
		}

		private void SwitchActiveClient(IntPtr foregroundClientHandle, string foregroundClientTitle)
		{
			// Check if any actions are needed
			if (this._activeClient.Handle == foregroundClientHandle)
			{
				return;
			}

			// Minimize the currently active client if needed
			if (this._activeClient.Handle != IntPtr.Zero
				&& this._configuration.MinimizeInactiveClients
				&& !this._configuration.IsPriorityClient(this._activeClient.Title))
			{
				this._windowManager.MinimizeWindow(this._activeClient.Handle, this._configuration.WindowsAnimationStyle, false);
				this._windowManager.ActivateWindow(foregroundClientHandle, this._configuration.WindowsAnimationStyle);
			}

			this._activeClient = (foregroundClientHandle, foregroundClientTitle);
		}

		private void ThumbnailViewFocused(IntPtr id)
		{
			if (this._ignoreViewEvents || this._isHoverEffectActive)
			{
				return;
			}

			if (!this._thumbnailViews.TryGetValue(id, out IThumbnailView view))
			{
				return;
			}

			this._isHoverEffectActive = true;

			view.SetTopMost(true);
			view.SetOpacity(1.0);

			if (this._configuration.ThumbnailZoomEnabled && !view.IsPreventPreviews())
			{
				this.ThumbnailZoomIn(view);
			}
		}

		private void ThumbnailViewLostFocus(IntPtr id)
		{
			if (this._ignoreViewEvents || !this._isHoverEffectActive)
			{
				return;
			}

			if (!this._thumbnailViews.TryGetValue(id, out IThumbnailView view))
			{
				this._isHoverEffectActive = false;
				return;
			}

			if (this._configuration.ThumbnailZoomEnabled)
			{
				this.ThumbnailZoomOut(view);
			}

			view.SetOpacity(this._configuration.ThumbnailOpacity);

			this._isHoverEffectActive = false;
		}

		private void ThumbnailActivated(IntPtr id)
		{
			if (this._ignoreViewEvents)
			{
				return;
			}

			if (!this._thumbnailViews.TryGetValue(id, out IThumbnailView view))
			{
				return;
			}

			Task.Run(() =>
				{
					this._windowManager.ActivateWindow(view.Id, this._configuration.WindowsAnimationStyle);
				})
				.ContinueWith((task) =>
				{
					if (!this._thumbnailViews.ContainsKey(id))
					{
						return;
					}

					// This code should be executed on UI thread
					this.SwitchActiveClient(view.Id, view.Title);
					this.UpdateClientLayouts();

					// Selecting a client from the preview grid dismisses the overview.
					if (this._previewOverviewActive)
					{
						this.ExitPreviewOverview();
					}

					this.RequestRefreshThumbnails();
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void ThumbnailDeactivated(IntPtr id, bool switchOut)
		{
			if (switchOut)
			{
				this._windowManager.ActivateWindow(this._externalApplication, this._configuration.WindowsAnimationStyle);
			}
			else
			{
				if (!this._thumbnailViews.TryGetValue(id, out IThumbnailView view))
				{
					return;
				}

				this._windowManager.MinimizeWindow(view.Id, this._configuration.WindowsAnimationStyle, true);
				this.RequestRefreshThumbnails();
			}
		}

		private void ThumbnailToggleCycleGroup(IntPtr id)
		{
			var view = GetClientByPointer(id);
			if ( view != null )
			{
				view.IsExcludedFromCycleGroup = !view.IsExcludedFromCycleGroup;
				view.SetCycleGroupIndicator(view.IsExcludedFromCycleGroup, _configuration.CycleGroupIndicatorAnchor);

			}
			this.RequestRefreshThumbnails();
		}


		private async void ThumbnailViewResized(IntPtr id)
		{
			if (this._ignoreViewEvents)
			{
				return;
			}

			if (!this._thumbnailViews.TryGetValue(id, out IThumbnailView view))
			{
				return;
			}

			this.SetThumbnailsSize(view.ThumbnailSize);

			view.Refresh(false);

			this._viewNotifier?.UpdateThumbnailSize(view.ThumbnailSize);
		}

		private void ThumbnailViewMoved(IntPtr id)
		{
			if (this._ignoreViewEvents)
			{
				return;
			}

			if (!this._thumbnailViews.TryGetValue(id, out IThumbnailView view))
			{
				return;
			}

			view.Refresh(false);
			this.EnqueueLocationChange(view);
		}

		// Checks whether currently active window belongs to an EVE client or its thumbnail
		private bool IsClientWindowActive(IntPtr windowHandle)
		{
			if (windowHandle == IntPtr.Zero)
			{
				return false;
			}

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				IThumbnailView view = entry.Value;

				if (view.IsKnownHandle(windowHandle))
				{
					return true;
				}
			}

			return false;
		}

		// Check whether the currently active window belongs to EVE-O-Preview itself
		private bool IsMainWindowActive(IntPtr windowHandle)
		{
			return (this._processMonitor.GetMainProcess().Handle == windowHandle);
		}

		private void ThumbnailZoomIn(IThumbnailView view)
		{
			this.DisableViewEvents();

			view.ZoomIn(ViewZoomAnchorConverter.Convert(view.ClientZoomAnchor), this._configuration.ThumbnailZoomFactor);
			view.Refresh(false);

			this.EnableViewEvents();
		}

		private void ThumbnailZoomOut(IThumbnailView view)
		{
			this.DisableViewEvents();

			view.ZoomOut();
			view.Refresh(false);

			this.EnableViewEvents();
		}

		private void SnapThumbnailView(IThumbnailView view)
		{
			// Check if this feature is enabled
			if (!this._configuration.EnableThumbnailSnap)
			{
				return;
			}

			// Only borderless thumbnails can be docked
			if (this._configuration.ShowThumbnailFrames)
			{
				return;
			}

			int width = this._configuration.ThumbnailSize.Width;
			int height = this._configuration.ThumbnailSize.Height;

			// TODO Extract method
			int baseX = view.ThumbnailLocation.X;
			int baseY = view.ThumbnailLocation.Y;

			Point[] viewPoints = { new Point(baseX, baseY), new Point(baseX + width, baseY), new Point(baseX, baseY + height), new Point(baseX + width, baseY + height) };

			// TODO Extract constants
			int thresholdX = Math.Max(20, width / 10);
			int thresholdY = Math.Max(20, height / 10);

			foreach (var entry in this._thumbnailViews)
			{
				IThumbnailView testView = entry.Value;

				if (view.Id == testView.Id)
				{
					continue;
				}

				int testX = testView.ThumbnailLocation.X;
				int testY = testView.ThumbnailLocation.Y;

				Point[] testPoints = { new Point(testX, testY), new Point(testX + width, testY), new Point(testX, testY + height), new Point(testX + width, testY + height) };

				var delta = ThumbnailManager.TestViewPoints(viewPoints, testPoints, thresholdX, thresholdY);

				if ((delta.X == 0) && (delta.Y == 0))
				{
					continue;
				}

				view.ThumbnailLocation = new Point(view.ThumbnailLocation.X + delta.X, view.ThumbnailLocation.Y + delta.Y);
				this._configuration.SetThumbnailLocation(view.Title, this._activeClient.Title, view.ThumbnailLocation);
				break;
			}
		}

		private static (int X, int Y) TestViewPoints(Point[] viewPoints, Point[] testPoints, int thresholdX, int thresholdY)
		{
			// Point combinations that we need to check
			// No need to check all 4x4 combinations
			(int ViewOffset, int TestOffset)[] testOffsets =
								{   ( 0, 3 ), ( 0, 2 ), ( 1, 2 ),
									( 0, 1 ), ( 0, 0 ), ( 1, 0 ),
									( 2, 1 ), ( 2, 0 ), ( 3, 0 )};

			foreach (var testOffset in testOffsets)
			{
				Point viewPoint = viewPoints[testOffset.ViewOffset];
				Point testPoint = testPoints[testOffset.TestOffset];

				int deltaX = testPoint.X - viewPoint.X;
				int deltaY = testPoint.Y - viewPoint.Y;

				if ((Math.Abs(deltaX) <= thresholdX) && (Math.Abs(deltaY) <= thresholdY))
				{
					return (deltaX, deltaY);
				}
			}

			return (0, 0);
		}
		private bool SetWindowStyle(IThumbnailView view, UInt32 styleToChange, bool remove)
		{
			IntPtr handle = view.Id;
			uint style = User32NativeMethods.GetWindowLong(handle, InteropConstants.GWL_STYLE);
			if (((style & styleToChange) == styleToChange) && remove == true)
			{
				style = style & ~styleToChange;
				User32NativeMethods.SetWindowLong(handle, InteropConstants.GWL_STYLE, style);
				return true;
			}
			if (((style & styleToChange) != styleToChange) && remove == false)
			{
				style = style | styleToChange;
				User32NativeMethods.SetWindowLong(handle, InteropConstants.GWL_STYLE, style);
				return true;
			}
			return false;
		}
		private void ApplyCaptionBar(IThumbnailView view)

		{
			if (view.Title == ThumbnailManager.DEFAULT_CLIENT_TITLE) return;
			IntPtr handle = view.Id;

			bool enable = this._configuration.HideCaptionOnClients;
			bool changed = false;
			changed = changed | SetWindowStyle(view, InteropConstants.WS_CAPTION, enable);
			changed = changed | SetWindowStyle(view, InteropConstants.WS_THICKFRAME, enable);
		}
		private void ApplyClientLayout(IThumbnailView view)
		{
			IntPtr clientHandle = view.Id;
			string clientTitle = view.Title;

			if (!this._configuration.EnableClientLayoutTracking)
			{
				return;
			}

			// No need to apply layout for not yet logged-in clients
			if (clientTitle == ThumbnailManager.DEFAULT_CLIENT_TITLE)
			{
				return;
			}

			ClientLayout clientLayout = this._configuration.GetClientLayout(clientTitle);

			if (clientLayout == null)
			{
				return;
			}

			if (clientLayout.IsMaximized)
			{
				this._windowManager.MaximizeWindow(clientHandle);
			}
			else
			{
				this._windowManager.MoveWindow(clientHandle, clientLayout.X, clientLayout.Y, clientLayout.Width, clientLayout.Height);
			}
		}

		private void UpdateClientLayouts()
		{
			if (!this._configuration.EnableClientLayoutTracking)
			{
				return;
			}

			foreach (KeyValuePair<IntPtr, IThumbnailView> entry in this._thumbnailViews)
			{
				IThumbnailView view = entry.Value;

				// No need to save layout for not yet logged-in clients
				if (view.Title == ThumbnailManager.DEFAULT_CLIENT_TITLE)
				{
					continue;
				}

				(int Left, int Top, int Right, int Bottom) position = this._windowManager.GetWindowPosition(view.Id);
				int width = Math.Abs(position.Right - position.Left);
				int height = Math.Abs(position.Bottom - position.Top);

				var isMaximized = this._windowManager.IsWindowMaximized(view.Id);

				if (!(isMaximized || this.IsValidWindowPosition(position.Left, position.Top, width, height)))
				{
					continue;
				}

				this._configuration.SetClientLayout(view.Title, new ClientLayout(position.Left, position.Top, width, height, isMaximized));
			}
		}

		private void EnqueueLocationChange(IThumbnailView view)
		{
			string activeClientTitle = this._activeClient.Title;
			// TODO ??
			this._configuration.SetThumbnailLocation(view.Title, activeClientTitle, view.ThumbnailLocation);

			lock (this._locationChangeNotificationSyncRoot)
			{
				if (this._enqueuedLocationChangeNotification.Handle == IntPtr.Zero)
				{
					this._enqueuedLocationChangeNotification = (view.Id, view.Title, activeClientTitle, view.ThumbnailLocation, ThumbnailManager.DEFAULT_LOCATION_CHANGE_NOTIFICATION_DELAY);
					return;
				}

				// Reset the delay and exit
				if ((this._enqueuedLocationChangeNotification.Handle == view.Id) &&
					(this._enqueuedLocationChangeNotification.ActiveClient == activeClientTitle))
				{
					this._enqueuedLocationChangeNotification.Delay = ThumbnailManager.DEFAULT_LOCATION_CHANGE_NOTIFICATION_DELAY;
					return;
				}

				this.RaiseThumbnailLocationUpdatedNotification(this._enqueuedLocationChangeNotification.Title);
				this._enqueuedLocationChangeNotification = (view.Id, view.Title, activeClientTitle, view.ThumbnailLocation, ThumbnailManager.DEFAULT_LOCATION_CHANGE_NOTIFICATION_DELAY);
			}
		}

		private bool TryDequeueLocationChange(out (IntPtr Handle, string Title, string ActiveClient, Point Location) change)
		{
			lock (this._locationChangeNotificationSyncRoot)
			{
				change = (IntPtr.Zero, null, null, Point.Empty);

				if (this._enqueuedLocationChangeNotification.Handle == IntPtr.Zero)
				{
					return false;
				}

				this._enqueuedLocationChangeNotification.Delay--;

				if (this._enqueuedLocationChangeNotification.Delay > 0)
				{
					return false;
				}

				change = (this._enqueuedLocationChangeNotification.Handle, this._enqueuedLocationChangeNotification.Title, this._enqueuedLocationChangeNotification.ActiveClient, this._enqueuedLocationChangeNotification.Location);
				this._enqueuedLocationChangeNotification = (IntPtr.Zero, null, null, Point.Empty, -1);

				return true;
			}
		}

		private void RaiseThumbnailLocationUpdatedNotification(string title)
		{
			if (string.IsNullOrEmpty(title) || (title == ThumbnailManager.DEFAULT_CLIENT_TITLE))
			{
				return;
			}

			this._configurationStorage.Save();
		}

		// We shouldn't manage some thumbnails (like thumbnail of the EVE client sitting on the login screen)
		// TODO Move to a service (?)
		private bool IsManageableThumbnail(IThumbnailView view)
		{
			return view.Title != ThumbnailManager.DEFAULT_CLIENT_TITLE;
		}

		private bool IsCycleEligible(IThumbnailView view)
		{
			return view.Id != IntPtr.Zero && !view.IsExcludedFromCycleGroup;
		}

		// Quick sanity check that the window is not minimized
		private Size GetAspectMatchedThumbnailSize(IntPtr handle, Size configuredSize)
		{
			Size clientSize = this._windowManager.GetClientAreaSize(handle);
			if (clientSize.Width < ThumbnailManager.WINDOW_SIZE_THRESHOLD || clientSize.Height < ThumbnailManager.WINDOW_SIZE_THRESHOLD)
			{
				return configuredSize;
			}

			int width = configuredSize.Width;
			int height = (int)Math.Round(width * (double)clientSize.Height / clientSize.Width);

			Size minimumSize = this._configuration.ThumbnailMinimumSize;
			Size maximumSize = this._configuration.ThumbnailMaximumSize;

			width = Math.Min(Math.Max(width, minimumSize.Width), maximumSize.Width);
			height = Math.Min(Math.Max(height, minimumSize.Height), maximumSize.Height);

			return new Size(width, height);
		}

		private bool IsValidWindowPosition(int left, int top, int width, int height)
		{
			return (left > ThumbnailManager.WINDOW_POSITION_THRESHOLD_LOW) && (left < ThumbnailManager.WINDOW_POSITION_THRESHOLD_HIGH)
					&& (top > ThumbnailManager.WINDOW_POSITION_THRESHOLD_LOW) && (top < ThumbnailManager.WINDOW_POSITION_THRESHOLD_HIGH)
					&& (width > ThumbnailManager.WINDOW_SIZE_THRESHOLD) && (height > ThumbnailManager.WINDOW_SIZE_THRESHOLD);
		}
	}
}

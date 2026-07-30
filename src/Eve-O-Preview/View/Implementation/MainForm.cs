using EveOPreview.Configuration;
using EveOPreview.Properties;
using EveOPreview.UI.Hotkeys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EveOPreview.View
{
	public partial class MainForm : Form, IMainFormView
	{
		private readonly ApplicationContext _context;
		private readonly InputBindingCapture _inputBindingCapture;
		private bool _suppressEvents;
		private Size _minimumSize;
		private Size _maximumSize;
		private string _iconName;
		private string _cycleForwardBinding;
		private string _minimizeAllBinding;
		private CycleBindingCaptureTarget _captureTarget;

		private double _thumbnailOpacity = 0.5;
		private bool _enableClientLayoutTracking;
		private bool _hideActiveClientThumbnail = true;
		private bool _showThumbnailsAlwaysOnTop = true;
		private bool _preventPreviews;
		private bool _hideThumbnailsOnLostFocus;
		private bool _enablePerClientThumbnailLayouts;
		private Size _thumbnailSize = new Size(384, 216);
		private bool _enableThumbnailZoom;
		private int _thumbnailZoomFactor = 2;
		private ViewZoomAnchor _thumbnailZoomAnchor = ViewZoomAnchor.NW;
		private ViewZoomAnchor _overlayLabelAnchor = ViewZoomAnchor.NW;
		private ViewZoomAnchor _cycleGroupIndicatorAnchor = ViewZoomAnchor.NW;
		private bool _showThumbnailOverlays = true;
		private bool _showThumbnailFrames;
		private bool _lockThumbnailLocation;
		private bool _thumbnailClickThrough;
		private bool _thumbnailSnapToGrid = true;
		private int _thumbnailSnapToGridSizeX = 100;
		private int _thumbnailSnapToGridSizeY = 50;
		private bool _enableActiveClientHighlight = true;
		private Color _activeClientHighlightColor = Color.GreenYellow;
		private Color _preventPreviewColor = Color.Purple;
		private Color _overlayLabelColor = Color.Orange;
		private Font _overlayLabelFont = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Bold);

		private enum CycleBindingCaptureTarget
		{
			None,
			Forward,
			MinimizeAll
		}

		public MainForm(ApplicationContext context)
		{
			this._context = context;
			this._minimumSize = new Size(20, 20);
			this._maximumSize = new Size(20, 20);

			InitializeComponent();

			this._inputBindingCapture = new InputBindingCapture();
			this._inputBindingCapture.Captured += this.InputBindingCapture_Captured;
			this._inputBindingCapture.CaptureCancelled += this.InputBindingCapture_Cancelled;

			this.ThumbnailsList.DisplayMember = "Title";
			this.AnimationStyleCombo.DataSource = Enum.GetValues(typeof(ViewAnimationStyle));

			MainFormTheme.Apply(this);
			this.RefreshCycleBindingCaptureState();
		}

		public bool MinimizeToTray
		{
			get => this.MinimizeToTrayCheckBox.Checked;
			set => this.MinimizeToTrayCheckBox.Checked = value;
		}

		public string IconName
		{
			get => this._iconName;
			set
			{
				this._iconName = value;

				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
				if (this._iconName == null || resources.GetObject(this._iconName) == null)
				{
					this._iconName = "IconOriginal";
				}

				try
				{
					var iconBytes = (byte[])resources.GetObject(this._iconName);
					using (MemoryStream ms = new MemoryStream(iconBytes))
					{
						this.Icon = new Icon(ms);
						this.NotifyIcon.Icon = this.Icon;
					}
				}
				catch
				{
					// Ignore missing icon resources.
				}

				if (!string.IsNullOrEmpty(value) && !this._suppressEvents)
				{
					this.ApplicationSettingsChanged?.Invoke();
				}
			}
		}

		public double ThumbnailOpacity
		{
			get => this._thumbnailOpacity;
			set => this._thumbnailOpacity = Math.Min(Math.Max(value, 0.1), 1.0);
		}

		public bool EnableClientLayoutTracking
		{
			get => this._enableClientLayoutTracking;
			set => this._enableClientLayoutTracking = value;
		}

		public bool HideActiveClientThumbnail
		{
			get => this._hideActiveClientThumbnail;
			set => this._hideActiveClientThumbnail = value;
		}

		public bool ShowThumbnailPreviews
		{
			get => false;
			set { }
		}

		public bool MinimizeInactiveClients
		{
			get => this.MinimizeInactiveClientsCheckBox.Checked;
			set => this.MinimizeInactiveClientsCheckBox.Checked = value;
		}

		public string CycleForwardBinding
		{
			get => this._cycleForwardBinding ?? string.Empty;
			set
			{
				this._cycleForwardBinding = value ?? string.Empty;
				this.CycleForwardBindingTextBox.Text = InputBindingHelper.ToDisplayString(this._cycleForwardBinding);
			}
		}

		public string MinimizeAllBinding
		{
			get => this._minimizeAllBinding ?? string.Empty;
			set
			{
				this._minimizeAllBinding = value ?? string.Empty;
				this.CycleBackwardBindingTextBox.Text = InputBindingHelper.ToDisplayString(this._minimizeAllBinding);
			}
		}

		public bool HideCaptionOnClients
		{
			get => this.HideCaptionOnClientsCheckBox.Checked;
			set => this.HideCaptionOnClientsCheckBox.Checked = value;
		}

		public ViewAnimationStyle WindowsAnimationStyle
		{
			get
			{
				if (this.AnimationStyleCombo.SelectedItem is ViewAnimationStyle style)
				{
					return style;
				}

				return ViewAnimationStyle.NoAnimation;
			}
			set
			{
				int index = (int)value;
				if (index >= 0 && index < this.AnimationStyleCombo.Items.Count)
				{
					this.AnimationStyleCombo.SelectedIndex = index;
				}
			}
		}

		public bool ShowThumbnailsAlwaysOnTop
		{
			get => this._showThumbnailsAlwaysOnTop;
			set => this._showThumbnailsAlwaysOnTop = value;
		}

		public bool PreventPreviews
		{
			get => this._preventPreviews;
			set => this._preventPreviews = value;
		}

		public bool HideThumbnailsOnLostFocus
		{
			get => this._hideThumbnailsOnLostFocus;
			set => this._hideThumbnailsOnLostFocus = value;
		}

		public bool EnablePerClientThumbnailLayouts
		{
			get => this._enablePerClientThumbnailLayouts;
			set => this._enablePerClientThumbnailLayouts = value;
		}

		public Size ThumbnailSize
		{
			get => this._thumbnailSize;
			set => this._thumbnailSize = value;
		}

		public bool EnableThumbnailZoom
		{
			get => this._enableThumbnailZoom;
			set => this._enableThumbnailZoom = value;
		}

		public int ThumbnailZoomFactor
		{
			get => this._thumbnailZoomFactor;
			set => this._thumbnailZoomFactor = value;
		}

		public ViewZoomAnchor ThumbnailZoomAnchor
		{
			get => this._thumbnailZoomAnchor;
			set => this._thumbnailZoomAnchor = value;
		}

		public ViewZoomAnchor OverlayLabelAnchor
		{
			get => this._overlayLabelAnchor;
			set => this._overlayLabelAnchor = value;
		}

		public ViewZoomAnchor CycleGroupIndicatorAnchor
		{
			get => this._cycleGroupIndicatorAnchor;
			set => this._cycleGroupIndicatorAnchor = value;
		}

		public bool ShowThumbnailOverlays
		{
			get => this._showThumbnailOverlays;
			set => this._showThumbnailOverlays = value;
		}

		public bool ShowThumbnailFrames
		{
			get => this._showThumbnailFrames;
			set => this._showThumbnailFrames = value;
		}

		public bool LockThumbnailLocation
		{
			get => this._lockThumbnailLocation;
			set => this._lockThumbnailLocation = value;
		}

		public bool ThumbnailClickThrough
		{
			get => this._thumbnailClickThrough;
			set => this._thumbnailClickThrough = value;
		}

		public void RefreshClickThroughCheckboxState()
		{
		}

		public void RefreshThumbnailDisplayOptionsState()
		{
		}

		public bool ThumbnailSnapToGrid
		{
			get => this._thumbnailSnapToGrid;
			set => this._thumbnailSnapToGrid = value;
		}

		public int ThumbnailSnapToGridSizeX
		{
			get => this._thumbnailSnapToGridSizeX;
			set => this._thumbnailSnapToGridSizeX = value;
		}

		public int ThumbnailSnapToGridSizeY
		{
			get => this._thumbnailSnapToGridSizeY;
			set => this._thumbnailSnapToGridSizeY = value;
		}

		public bool EnableActiveClientHighlight
		{
			get => this._enableActiveClientHighlight;
			set => this._enableActiveClientHighlight = value;
		}

		public Color ActiveClientHighlightColor
		{
			get => this._activeClientHighlightColor;
			set => this._activeClientHighlightColor = value;
		}

		public Color PreventPreviewColor
		{
			get => this._preventPreviewColor;
			set => this._preventPreviewColor = value;
		}

		public Color OverlayLabelColor
		{
			get => this._overlayLabelColor;
			set => this._overlayLabelColor = value;
		}

		public Font OverlayLabelFont
		{
			get => this._overlayLabelFont;
			set => this._overlayLabelFont = value;
		}

		public new void Show()
		{
			this._context.MainForm = this;

			this._suppressEvents = true;
			this.FormActivated?.Invoke();
			this._suppressEvents = false;

			Application.Run(this._context);
		}

		public void SetThumbnailSizeLimitations(Size minimumSize, Size maximumSize)
		{
			this._minimumSize = minimumSize;
			this._maximumSize = maximumSize;
		}

		public void Minimize()
		{
			this.WindowState = FormWindowState.Minimized;
		}

		public void SetVersionInfo(string version)
		{
			this.VersionLabel.Text = version;
		}

		public void SetDocumentationUrl(string url)
		{
			this.DocumentationLink.Text = url;
		}

		public void AddThumbnails(IList<IThumbnailDescription> thumbnails)
		{
			this.ThumbnailsList.BeginUpdate();

			foreach (IThumbnailDescription view in thumbnails)
			{
				this.ThumbnailsList.SetItemChecked(this.ThumbnailsList.Items.Add(view), view.IsDisabled);
			}

			this.ThumbnailsList.EndUpdate();
		}

		public void RemoveThumbnails(IList<IThumbnailDescription> thumbnails)
		{
			this.ThumbnailsList.BeginUpdate();

			foreach (IThumbnailDescription view in thumbnails)
			{
				this.ThumbnailsList.Items.Remove(view);
			}

			this.ThumbnailsList.EndUpdate();
		}

		public void RefreshZoomSettings()
		{
		}

		public void RefreshCycleBindingCaptureState()
		{
			bool isCapturing = this._inputBindingCapture.IsCapturing;
			this.CycleForwardRecordButton.Enabled = !isCapturing || this._captureTarget == CycleBindingCaptureTarget.Forward;
			this.CycleBackwardRecordButton.Enabled = !isCapturing || this._captureTarget == CycleBindingCaptureTarget.MinimizeAll;
			this.CycleForwardBindingTextBox.BackColor = this._captureTarget == CycleBindingCaptureTarget.Forward
				? MainFormTheme.CaptureHighlight
				: MainFormTheme.Input;
			this.CycleBackwardBindingTextBox.BackColor = this._captureTarget == CycleBindingCaptureTarget.MinimizeAll
				? MainFormTheme.CaptureHighlight
				: MainFormTheme.Input;
			this.CycleForwardRecordButton.BackColor = this._captureTarget == CycleBindingCaptureTarget.Forward
				? MainFormTheme.CaptureHighlight
				: MainFormTheme.Button;
			this.CycleBackwardRecordButton.BackColor = this._captureTarget == CycleBindingCaptureTarget.MinimizeAll
				? MainFormTheme.CaptureHighlight
				: MainFormTheme.Button;
		}

		public Action ApplicationExitRequested { get; set; }
		public Action FormActivated { get; set; }
		public Action FormMinimized { get; set; }
		public Action<ViewCloseRequest> FormCloseRequested { get; set; }
		public Action ApplicationSettingsChanged { get; set; }
		public Action ThumbnailsSizeChanged { get; set; }
		public Action<string> ThumbnailStateChanged { get; set; }
		public Action DocumentationLinkActivated { get; set; }

		private void CycleForwardRecordButton_Click(object sender, EventArgs e)
		{
			this.StartBindingCapture(CycleBindingCaptureTarget.Forward);
		}

		private void CycleBackwardRecordButton_Click(object sender, EventArgs e)
		{
			this.StartBindingCapture(CycleBindingCaptureTarget.MinimizeAll);
		}

		private void StartBindingCapture(CycleBindingCaptureTarget target)
		{
			if (this._inputBindingCapture.IsCapturing && this._captureTarget == target)
			{
				this.StopBindingCapture();
				return;
			}

			this._captureTarget = target;
			TextBox targetTextBox = target == CycleBindingCaptureTarget.Forward
				? this.CycleForwardBindingTextBox
				: this.CycleBackwardBindingTextBox;
			targetTextBox.Text = "Press a key, combo, or mouse button...";

			this._inputBindingCapture.Start();
			this.RefreshCycleBindingCaptureState();
		}

		private void StopBindingCapture()
		{
			this._inputBindingCapture.Stop();
			this._captureTarget = CycleBindingCaptureTarget.None;
			this.RefreshCycleBindingCaptureState();
		}

		private void InputBindingCapture_Captured(object sender, string binding)
		{
			if (this._captureTarget == CycleBindingCaptureTarget.Forward)
			{
				this.CycleForwardBinding = binding;
			}
			else if (this._captureTarget == CycleBindingCaptureTarget.MinimizeAll)
			{
				this.MinimizeAllBinding = binding;
			}

			this._captureTarget = CycleBindingCaptureTarget.None;
			this.RefreshCycleBindingCaptureState();
			this.OptionChanged_Handler(this, EventArgs.Empty);
		}

		private void InputBindingCapture_Cancelled(object sender, EventArgs e)
		{
			if (this._captureTarget == CycleBindingCaptureTarget.Forward)
			{
				this.CycleForwardBinding = this._cycleForwardBinding;
			}
			else if (this._captureTarget == CycleBindingCaptureTarget.MinimizeAll)
			{
				this.MinimizeAllBinding = this._minimizeAllBinding;
			}

			this._captureTarget = CycleBindingCaptureTarget.None;
			this.RefreshCycleBindingCaptureState();
		}

		private void OptionChanged_Handler(object sender, EventArgs e)
		{
			if (this._suppressEvents)
			{
				return;
			}

			this.ApplicationSettingsChanged?.Invoke();
		}

		private void ThumbnailsList_ItemCheck_Handler(object sender, ItemCheckEventArgs e)
		{
			if (!(this.ThumbnailsList.Items[e.Index] is IThumbnailDescription selectedItem))
			{
				return;
			}

			selectedItem.IsDisabled = (e.NewValue == CheckState.Checked);
			this.ThumbnailStateChanged?.Invoke(selectedItem.Title);
		}

		private void DocumentationLinkClicked_Handler(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.DocumentationLinkActivated?.Invoke();
		}

		private void MainFormResize_Handler(object sender, EventArgs e)
		{
			if (this.WindowState != FormWindowState.Minimized)
			{
				return;
			}

			this.FormMinimized?.Invoke();
		}

		private void MainFormClosing_Handler(object sender, FormClosingEventArgs e)
		{
			this.StopBindingCapture();

			ViewCloseRequest request = new ViewCloseRequest();
			this.FormCloseRequested?.Invoke(request);
			e.Cancel = !request.Allow;
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			this._inputBindingCapture.Dispose();
			base.OnFormClosed(e);
		}

		private void RestoreMainForm_Handler(object sender, EventArgs e)
		{
			base.Show();
			this.WindowState = FormWindowState.Normal;
			this.BringToFront();
		}

		private void ExitMenuItemClick_Handler(object sender, EventArgs e)
		{
			this.ApplicationExitRequested?.Invoke();
		}
	}
}

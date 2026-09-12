using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace EveOPreview.Configuration.Implementation
{
	sealed class ThumbnailConfiguration : IThumbnailConfiguration
	{
		#region Private fields
		private bool _enablePerClientThumbnailLayouts;
		private bool _enableClientLayoutTracking;
		#endregion

		public ThumbnailConfiguration()
		{
			this.ConfigVersion = 3;

			this.CycleGroup1ForwardHotkeys = new List<string> { "F14", "MouseXButton1" };

			this.PerClientActiveClientHighlightColor = new Dictionary<string, Color>();
			this.PerClientPreventPreviewColor = new Dictionary<string, Color>();
			this.PerClientPreventPreviews = new Dictionary<string, bool>();
			this.PerClientThumbnailSize = new Dictionary<string, Size>();
			this.PerClientZoomAnchor = new Dictionary<string, ZoomAnchor>();

			this.PerClientLayout = new Dictionary<string, Dictionary<string, Point>>();
			this.FlatLayout = new Dictionary<string, Point>();
			this.ClientLayout = new Dictionary<string, ClientLayout>();
			this.ClientHotkey = new Dictionary<string, string>();
			this.MinimizeAllClientsHotkeys = new List<string> { "MouseXButton2" };
			this.ShowAllPreviewsHotkeys = new List<string> { "Pause" };
			this.DisableThumbnail = new Dictionary<string, bool>();
			this.PriorityClients = new List<string>();

			this.ExecutablesToPreview = new List<string> { "exefile" };

			this.MinimizeToTray = false;
			this.ThumbnailRefreshPeriod = 500;
			this.ThumbnailResizeTimeoutPeriod = 500;

			this.ThumbnailOpacity = 0.5;

			this.EnableClientLayoutTracking = false;
			this.HideActiveClientThumbnail = true;
			this.ShowThumbnailPreviews = false;
			this.HideLoginClientThumbnail = false;
			this.MinimizeInactiveClients = true;
			this.HideCaptionOnClients = false;
			this.WindowsAnimationStyle = AnimationStyle.NoAnimation;
			this.ShowThumbnailsAlwaysOnTop = true;
			this.EnablePerClientThumbnailLayouts = false;

			this.HideThumbnailsOnLostFocus = false;
			this.PreventPreviews = false;
			this.HideThumbnailsDelay = 2; // 2 thumbnails refresh cycles (1.0 sec)

			this.ThumbnailSize = new Size(384, 216);
			this.ThumbnailMinimumSize = new Size(192, 108);
			this.ThumbnailMaximumSize = new Size(960, 540);

			this.EnableThumbnailSnap = true;

			this.ThumbnailZoomEnabled = false;
			this.ThumbnailZoomFactor = 2;
			this.ThumbnailZoomAnchor = ZoomAnchor.NW;
			this.OverlayLabelAnchor = ZoomAnchor.NW;
			this.CycleGroupIndicatorAnchor = ZoomAnchor.NW;

			this.ShowThumbnailOverlays = true;
			this.ShowThumbnailFrames = false;
			this.LockThumbnailLocation = false;

			this.ThumbnailSnapToGrid = true;
			this.ThumbnailSnapToGridSizeX = 100;
			this.ThumbnailSnapToGridSizeY = 50;

            this.EnableActiveClientHighlight = true;
			this.ActiveClientHighlightColor = Color.GreenYellow;
			this.PreventPreviewColor = Color.Purple;
			this.ActiveClientHighlightThickness = 3;

			this.OverlayLabelColor = Color.Orange;
			this.OverlayLabelFont = new Font(FontFamily.GenericSansSerif,10.0F, FontStyle.Bold);

			this.IconName = "";

			this.LoginThumbnailLocation = new Point(5, 5);
		}


		[JsonProperty("ConfigVersion")]
		public int ConfigVersion { get; set; }

		[JsonProperty("CycleGroup1ForwardHotkeys")]
		public List<string> CycleGroup1ForwardHotkeys { get; set; }

		[JsonProperty("PerClientPreventPreviewColor")]
		public Dictionary<string, Color> PerClientPreventPreviewColor { get; set; }

		[JsonProperty("PerClientActiveClientHighlightColor")]
		public Dictionary<string, Color> PerClientActiveClientHighlightColor { get; set; }

		[JsonProperty("PerClientPreventPreviews")]
		public Dictionary<string, bool> PerClientPreventPreviews { get; set; }

		[JsonProperty("PerClientThumbnailSize")]
		public Dictionary<string, Size> PerClientThumbnailSize { get; set; }

		[JsonProperty("PerClientZoomAnchor")]
		public Dictionary<string, ZoomAnchor> PerClientZoomAnchor{ get; set; }
		public bool MinimizeToTray { get; set; }
		public int ThumbnailRefreshPeriod { get; set; }
		public int ThumbnailResizeTimeoutPeriod { get; set; }

		[JsonProperty("ThumbnailsOpacity")]
		public double ThumbnailOpacity { get; set; }

		public bool EnableClientLayoutTracking
		{
			get => this._enableClientLayoutTracking;
			set
			{
				if (!value)
				{
					this.ClientLayout.Clear();
				}

				this._enableClientLayoutTracking = value;
			}
		}

		public bool HideActiveClientThumbnail { get; set; }
		public bool ShowThumbnailPreviews { get; set; }
		public bool HideLoginClientThumbnail { get; set; }
		public bool MinimizeInactiveClients { get; set; }
		public bool HideCaptionOnClients { get; set; }
		public AnimationStyle WindowsAnimationStyle { get; set; }
		public bool ShowThumbnailsAlwaysOnTop { get; set; }

		public bool EnablePerClientThumbnailLayouts
		{
			get => this._enablePerClientThumbnailLayouts;
			set
			{
				if (!value)
				{
					this.PerClientLayout.Clear();
				}

				this._enablePerClientThumbnailLayouts = value;
			}
		}

		public bool PreventPreviews { get; set; }
		public bool HideThumbnailsOnLostFocus { get; set; }
		public int HideThumbnailsDelay { get; set; }

		public Size ThumbnailSize { get; set; }
		public Size ThumbnailMaximumSize { get; set; }
		public Size ThumbnailMinimumSize { get; set; }

		public bool EnableThumbnailSnap { get; set; }

		[JsonProperty("EnableThumbnailZoom")]
		public bool ThumbnailZoomEnabled { get; set; }
		public int ThumbnailZoomFactor { get; set; }
		public ZoomAnchor ThumbnailZoomAnchor { get; set; }
		public ZoomAnchor OverlayLabelAnchor { get; set; }
		public ZoomAnchor CycleGroupIndicatorAnchor { get; set; }

		public bool ShowThumbnailOverlays { get; set; }
		public bool ShowThumbnailFrames { get; set; }
		public bool LockThumbnailLocation { get; set; }
		public bool ThumbnailClickThrough { get; set; }
		public bool ThumbnailSnapToGrid { get; set; }
		public int ThumbnailSnapToGridSizeX {  get; set; }
		public int ThumbnailSnapToGridSizeY { get; set; }

		public bool EnableActiveClientHighlight { get; set; }

		public Color ActiveClientHighlightColor { get; set; }
		public Color PreventPreviewColor { get; set; }
		public Color OverlayLabelColor { get; set; }

		[JsonProperty]
		public Font OverlayLabelFont { get; set; }
		public string IconName { get; set; }

		public int ActiveClientHighlightThickness { get; set; }

		[JsonProperty("LoginThumbnailLocation")]
		public Point LoginThumbnailLocation { get; set; }

		[JsonProperty]
		private Dictionary<string, Dictionary<string, Point>> PerClientLayout { get; set; }
		[JsonProperty]
		private Dictionary<string, Point> FlatLayout { get; set; }
		[JsonProperty]
		private Dictionary<string, ClientLayout> ClientLayout { get; set; }
		[JsonProperty]
		private Dictionary<string, string> ClientHotkey { get; set; }
		[JsonProperty]
		public List<string> MinimizeAllClientsHotkeys { get; set; }
		[JsonProperty]
		public List<string> ShowAllPreviewsHotkeys { get; set; }
		[JsonProperty]
		private Dictionary<string, bool> DisableThumbnail { get; set; }
		[JsonProperty]
		private List<string> PriorityClients { get; set; }
		[JsonProperty]
		public List<string> ExecutablesToPreview { get; set; }

		IReadOnlyList<string> IThumbnailConfiguration.ExecutablesToPreview => this.ExecutablesToPreview;

		public Point GetThumbnailLocation(string currentClient, string activeClient, Point defaultLocation)
		{
			Point location;

			// What this code does:
			// If Per-Client layouts are enabled
			//    and client name is known
			//    and there is a separate thumbnails layout for this client
			//    and this layout contains an entry for the current client
			// then return that entry
			// otherwise try to get client layout from the flat all-clients layout
			// If there is no layout too then use the default one
			if (this.EnablePerClientThumbnailLayouts && !string.IsNullOrEmpty(activeClient))
			{
				Dictionary<string, Point> layoutSource;
				if (this.PerClientLayout.TryGetValue(activeClient, out layoutSource) && layoutSource.TryGetValue(currentClient, out location))
				{
					return location;
				}
			}

			return this.FlatLayout.TryGetValue(currentClient, out location) ? location : defaultLocation;
		}

		public Size GetThumbnailSize(string currentClient, string activeClient, Size defaultSize)
		{
			Size sizeOfThumbnail;
			return this.PerClientThumbnailSize.TryGetValue(currentClient, out sizeOfThumbnail) ? sizeOfThumbnail : defaultSize;
		}
		public ZoomAnchor GetZoomAnchor(string currentClient, ZoomAnchor defaultZoomAnchor)
		{
			ZoomAnchor zoomAnchor;
			return this.PerClientZoomAnchor.TryGetValue(currentClient, out zoomAnchor) ? zoomAnchor : defaultZoomAnchor;
		}

		public void SetThumbnailLocation(string currentClient, string activeClient, Point location)
		{
			Dictionary<string, Point> layoutSource;

			if (this.EnablePerClientThumbnailLayouts)
			{
				if (string.IsNullOrEmpty(activeClient))
				{
					return;
				}

				if (!this.PerClientLayout.TryGetValue(activeClient, out layoutSource))
				{
					layoutSource = new Dictionary<string, Point>();
					this.PerClientLayout[activeClient] = layoutSource;
				}
			}
			else
			{
				layoutSource = this.FlatLayout;
			}

			layoutSource[currentClient] = location;
		}

		public ClientLayout GetClientLayout(string currentClient)
		{
			ClientLayout layout;
			this.ClientLayout.TryGetValue(currentClient, out layout);

			return layout;
		}

		public void SetClientLayout(string currentClient, ClientLayout layout)
		{
			this.ClientLayout[currentClient] = layout;
		}

		public Keys GetClientHotkey(string currentClient)
		{
			string hotkey;
			if (this.ClientHotkey.TryGetValue(currentClient, out hotkey))
			{
				// Protect from incorrect values
				object rawValue = (new KeysConverter()).ConvertFromInvariantString(hotkey);
				return rawValue != null ? (Keys)rawValue : Keys.None;
			}

			return Keys.None;
		}

		public void SetClientHotkey(string currentClient, Keys hotkey)
		{
			this.ClientHotkey[currentClient] = (new KeysConverter()).ConvertToInvariantString(hotkey);
		}

		public Keys StringToKey(string hotkey)
		{
			object rawValue = (new KeysConverter()).ConvertFromInvariantString(hotkey);
			return rawValue != null ? (Keys)rawValue : Keys.None;
		}

		public bool IsPriorityClient(string currentClient)
		{
			return this.PriorityClients.Contains(currentClient);
		}
		public bool IsExecutableToPreview(string processName)
		{
			return this.ExecutablesToPreview.Any(s => s.Equals(processName, StringComparison.OrdinalIgnoreCase));
		}

		public bool IsThumbnailDisabled(string currentClient)
		{
			return this.DisableThumbnail.TryGetValue(currentClient, out bool isDisabled) && isDisabled;
		}

		public void ToggleThumbnail(string currentClient, bool isDisabled)
		{
			this.DisableThumbnail[currentClient] = isDisabled;
		}

		/// <summary>
		/// Applies restrictions to different parameters of the config
		/// </summary>
		public void ApplyRestrictions()
		{
			if (this.ConfigVersion < 2)
			{
				this.MinimizeInactiveClients = true;
				this.ConfigVersion = 2;
			}

			if (this.ConfigVersion < 3)
			{
				this.MinimizeInactiveClients = true;
				this.WindowsAnimationStyle = AnimationStyle.NoAnimation;
				this.HideActiveClientThumbnail = true;
				this.EnableActiveClientHighlight = true;
				this.ConfigVersion = 3;
			}

			this.EnsureDefaultMouseActionBindings();

			if (this.ShowAllPreviewsHotkeys == null)
			{
				this.ShowAllPreviewsHotkeys = new List<string>();
			}

			if (!this.LockThumbnailLocation)
			{
				this.ThumbnailClickThrough = false;
			}
			this.ThumbnailRefreshPeriod = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailRefreshPeriod, 300, 1000);
			this.ThumbnailResizeTimeoutPeriod = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailResizeTimeoutPeriod, 200, 5000);
			this.ThumbnailSize = new Size(ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailSize.Width, this.ThumbnailMinimumSize.Width, this.ThumbnailMaximumSize.Width),
				ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailSize.Height, this.ThumbnailMinimumSize.Height, this.ThumbnailMaximumSize.Height));
			this.ThumbnailOpacity = ThumbnailConfiguration.ApplyRestrictions((int)(this.ThumbnailOpacity * 100.00), 20, 100) / 100.00;
			this.ThumbnailZoomFactor = ThumbnailConfiguration.ApplyRestrictions(this.ThumbnailZoomFactor, 2, 10);
			this.ActiveClientHighlightThickness = ThumbnailConfiguration.ApplyRestrictions(this.ActiveClientHighlightThickness, 1, 6);
		}

		private static int ApplyRestrictions(int value, int minimum, int maximum)
		{
			if (value <= minimum)
			{
				return minimum;
			}

			if (value >= maximum)
			{
				return maximum;
			}

			return value;
		}

		private void EnsureDefaultMouseActionBindings()
		{
			if (this.CycleGroup1ForwardHotkeys != null
				&& !this.CycleGroup1ForwardHotkeys.Any(ThumbnailConfiguration.IsMouseBinding))
			{
				this.CycleGroup1ForwardHotkeys.Add("MouseXButton1");
			}

			if (this.MinimizeAllClientsHotkeys != null
				&& !this.MinimizeAllClientsHotkeys.Any(ThumbnailConfiguration.IsMouseBinding))
			{
				this.MinimizeAllClientsHotkeys.Add("MouseXButton2");
			}
		}

		private static bool IsMouseBinding(string binding)
		{
			return !string.IsNullOrWhiteSpace(binding)
				&& binding.Trim().StartsWith("Mouse", StringComparison.OrdinalIgnoreCase);
		}
	}
}
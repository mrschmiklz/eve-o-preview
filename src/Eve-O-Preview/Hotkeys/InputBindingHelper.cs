using System;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys
{
	enum InputBindingKind
	{
		None,
		Keyboard,
		Mouse
	}

	static class InputBindingHelper
	{
		public const string MouseXButton1 = "MouseXButton1";
		public const string MouseXButton2 = "MouseXButton2";
		public const string MouseMiddle = "MouseMiddle";

		public static InputBindingKind GetKind(string binding)
		{
			if (string.IsNullOrWhiteSpace(binding))
			{
				return InputBindingKind.None;
			}

			switch (binding.Trim())
			{
				case MouseXButton1:
				case MouseXButton2:
				case MouseMiddle:
					return InputBindingKind.Mouse;
				default:
					return InputBindingKind.Keyboard;
			}
		}

		public static Keys ParseKeyboardBinding(string binding)
		{
			if (GetKind(binding) != InputBindingKind.Keyboard)
			{
				return Keys.None;
			}

			object rawValue = (new KeysConverter()).ConvertFromInvariantString(binding);
			return rawValue != null ? (Keys)rawValue : Keys.None;
		}

		public static string FormatKeyboardBinding(Keys keyData)
		{
			if (keyData == Keys.None)
			{
				return string.Empty;
			}

			return (new KeysConverter()).ConvertToInvariantString(keyData);
		}

		public static string FormatMouseBinding(MappedMouseButton button)
		{
			switch (button)
			{
				case MappedMouseButton.SideButton1:
					return MouseXButton1;
				case MappedMouseButton.SideButton2:
					return MouseXButton2;
				case MappedMouseButton.MiddleButton:
					return MouseMiddle;
				default:
					return string.Empty;
			}
		}

		public static bool TryParseMouseBinding(string binding, out MappedMouseButton button)
		{
			switch (binding?.Trim())
			{
				case MouseXButton1:
					button = MappedMouseButton.SideButton1;
					return true;
				case MouseXButton2:
					button = MappedMouseButton.SideButton2;
					return true;
				case MouseMiddle:
					button = MappedMouseButton.MiddleButton;
					return true;
				default:
					button = default;
					return false;
			}
		}

		public static string ToDisplayString(string binding)
		{
			if (string.IsNullOrWhiteSpace(binding))
			{
				return "Not set";
			}

			switch (binding.Trim())
			{
				case MouseXButton1:
					return "Mouse Button 4 (Rear)";
				case MouseXButton2:
					return "Mouse Button 5 (Front)";
				case MouseMiddle:
					return "Middle Click";
				default:
					return binding.Replace("Control+", "Ctrl+", StringComparison.OrdinalIgnoreCase);
			}
		}
	}
}

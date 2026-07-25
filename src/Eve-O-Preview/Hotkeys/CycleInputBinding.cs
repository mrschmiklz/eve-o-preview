#if !LINUX
using System;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys
{
	sealed class CycleInputBinding : IDisposable
	{
		private HotkeyHandler _keyboardHandler;
		private string _binding;

		public string Binding => this._binding;

		public void Register(string binding, Action onPressed, GlobalMouseInputHandler mouseInputHandler)
		{
			this.Unregister(mouseInputHandler);

			this._binding = binding;
			if (string.IsNullOrWhiteSpace(binding) || onPressed == null)
			{
				return;
			}

			switch (InputBindingHelper.GetKind(binding))
			{
				case InputBindingKind.Keyboard:
					Keys key = InputBindingHelper.ParseKeyboardBinding(binding);
					if (key == Keys.None)
					{
						return;
					}

					this._keyboardHandler = new HotkeyHandler(default(IntPtr), key);
					this._keyboardHandler.Pressed += (sender, args) =>
					{
						onPressed();
						args.Handled = true;
					};
					this._keyboardHandler.Register();
					break;
				case InputBindingKind.Mouse:
					mouseInputHandler.Register(binding, onPressed);
					break;
			}
		}

		public void Unregister(GlobalMouseInputHandler mouseInputHandler)
		{
			this._keyboardHandler?.Dispose();
			this._keyboardHandler = null;

			if (!string.IsNullOrWhiteSpace(this._binding))
			{
				mouseInputHandler?.Unregister(this._binding);
			}

			this._binding = null;
		}

		public void Dispose()
		{
			this._keyboardHandler?.Dispose();
			this._keyboardHandler = null;
			this._binding = null;
		}
	}
}
#endif

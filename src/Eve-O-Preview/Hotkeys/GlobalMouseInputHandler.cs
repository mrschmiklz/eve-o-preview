#if !LINUX
using System;
using System.Collections.Generic;

namespace EveOPreview.UI.Hotkeys
{
	sealed class GlobalMouseInputHandler : IDisposable
	{
		private readonly SideMouseButtonHandler _mouseHandler;
		private readonly Dictionary<string, Action> _bindingActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

		public GlobalMouseInputHandler()
		{
			this._mouseHandler = new SideMouseButtonHandler();
			this._mouseHandler.SideButtonPressed += this.OnSideButtonPressed;
		}

		public bool IsRegistered => this._mouseHandler.IsRegistered;

		public void Register(string binding, Action action)
		{
			if (string.IsNullOrWhiteSpace(binding) || action == null)
			{
				return;
			}

			if (InputBindingHelper.GetKind(binding) != InputBindingKind.Mouse)
			{
				return;
			}

			this._bindingActions[binding.Trim()] = action;

			if (!this._mouseHandler.IsRegistered)
			{
				this._mouseHandler.Register();
			}
		}

		public void Unregister(string binding)
		{
			if (string.IsNullOrWhiteSpace(binding))
			{
				return;
			}

			this._bindingActions.Remove(binding.Trim());

			if (this._bindingActions.Count == 0)
			{
				this._mouseHandler.Unregister();
			}
		}

		public void Clear()
		{
			this._bindingActions.Clear();
			this._mouseHandler.Unregister();
		}

		public void Dispose()
		{
			this.Clear();
			this._mouseHandler.SideButtonPressed -= this.OnSideButtonPressed;
			this._mouseHandler.Dispose();
		}

		private void OnSideButtonPressed(object sender, SideMouseButtonEventArgs eventArgs)
		{
			foreach (KeyValuePair<string, Action> entry in this._bindingActions)
			{
				if (!InputBindingHelper.TryParseMouseBinding(entry.Key, out MappedMouseButton button))
				{
					continue;
				}

				if (button != eventArgs.Button)
				{
					continue;
				}

				eventArgs.Handled = true;
				entry.Value.Invoke();
				return;
			}
		}
	}
}
#endif

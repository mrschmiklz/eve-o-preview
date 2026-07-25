using System.Windows.Forms;
using EveOPreview.Services.Interop;

namespace EveOPreview.Services
{
	static class ClickThroughHelper
	{
		private const int GwlExstyle = -20;

		public static void SetClickThrough(Control control, bool enable)
		{
			if (control == null)
			{
				return;
			}

			ApplyRecursive(control, enable);
		}

		private static void ApplyRecursive(Control control, bool enable)
		{
			ApplyStyle(control, enable);

			foreach (Control child in control.Controls)
			{
				ApplyRecursive(child, enable);
			}
		}

		private static void ApplyStyle(Control control, bool enable)
		{
			if (!control.IsHandleCreated)
			{
				control.HandleCreated += (_, _) => ApplyStyle(control, enable);
				return;
			}

			uint style = User32NativeMethods.GetWindowLong(control.Handle, GwlExstyle);
			if (enable)
			{
				style |= InteropConstants.WS_EX_TRANSPARENT;
			}
			else
			{
				style &= ~InteropConstants.WS_EX_TRANSPARENT;
			}

			User32NativeMethods.SetWindowLong(control.Handle, GwlExstyle, style);
		}
	}
}

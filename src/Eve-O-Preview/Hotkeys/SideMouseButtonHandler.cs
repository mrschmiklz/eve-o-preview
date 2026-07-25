#if !LINUX
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EveOPreview.UI.Hotkeys
{
	delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

	enum MappedMouseButton
	{
		SideButton1,
		SideButton2,
		MiddleButton
	}

	sealed class SideMouseButtonEventArgs : HandledEventArgs
	{
		public SideMouseButtonEventArgs(MappedMouseButton button)
		{
			this.Button = button;
		}

		public MappedMouseButton Button { get; }
	}

	sealed class SideMouseButtonHandler : IDisposable
	{
		private const int WH_MOUSE_LL = 14;
		private const int WM_XBUTTONDOWN = 0x020B;
		private const int WM_MBUTTONDOWN = 0x0207;
		private const int XBUTTON1 = 0x0001;
		private const int XBUTTON2 = 0x0002;

		private readonly LowLevelMouseProc _hookProc;
		private IntPtr _hookId = IntPtr.Zero;

		public SideMouseButtonHandler()
		{
			this._hookProc = this.HookCallback;
		}

		public bool IsRegistered { get; private set; }

		public event EventHandler<SideMouseButtonEventArgs> SideButtonPressed;

		public bool Register()
		{
			if (this.IsRegistered)
			{
				return false;
			}

			using (Process currentProcess = Process.GetCurrentProcess())
			using (ProcessModule currentModule = currentProcess.MainModule)
			{
				this._hookId = SideMouseButtonHandlerNativeMethods.SetWindowsHookEx(
					SideMouseButtonHandlerNativeMethods.WH_MOUSE_LL,
					this._hookProc,
					SideMouseButtonHandlerNativeMethods.GetModuleHandle(currentModule.ModuleName),
					0);
			}

			this.IsRegistered = this._hookId != IntPtr.Zero;
			return this.IsRegistered;
		}

		public void Unregister()
		{
			if (!this.IsRegistered)
			{
				return;
			}

			SideMouseButtonHandlerNativeMethods.UnhookWindowsHookEx(this._hookId);
			this._hookId = IntPtr.Zero;
			this.IsRegistered = false;
		}

		public void Dispose()
		{
			this.Unregister();
			GC.SuppressFinalize(this);
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				MappedMouseButton? button = null;

				if (wParam == (IntPtr)WM_XBUTTONDOWN)
				{
					MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
					int xButton = (hookStruct.mouseData >> 16) & 0xFFFF;

					if (xButton == XBUTTON1)
					{
						button = MappedMouseButton.SideButton1;
					}
					else if (xButton == XBUTTON2)
					{
						button = MappedMouseButton.SideButton2;
					}
				}
				else if (wParam == (IntPtr)WM_MBUTTONDOWN)
				{
					button = MappedMouseButton.MiddleButton;
				}

				if (button.HasValue)
				{
					SideMouseButtonEventArgs eventArgs = new SideMouseButtonEventArgs(button.Value);
					this.SideButtonPressed?.Invoke(this, eventArgs);

					if (eventArgs.Handled)
					{
						return (IntPtr)1;
					}
				}
			}

			return SideMouseButtonHandlerNativeMethods.CallNextHookEx(this._hookId, nCode, wParam, lParam);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MSLLHOOKSTRUCT
		{
			public POINT pt;
			public int mouseData;
			public int flags;
			public int time;
			public IntPtr dwExtraInfo;
		}
	}

	static class SideMouseButtonHandlerNativeMethods
	{
		public const int WH_MOUSE_LL = 14;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}
#endif

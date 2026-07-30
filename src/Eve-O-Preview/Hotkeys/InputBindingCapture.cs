using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys
{
	delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	sealed class InputBindingCapture : IDisposable
	{
		private const int WH_KEYBOARD_LL = 13;
		private const int WH_MOUSE_LL = 14;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_XBUTTONDOWN = 0x020B;
		private const int WM_MBUTTONDOWN = 0x0207;
		private const int XBUTTON1 = 0x0001;
		private const int XBUTTON2 = 0x0002;
		private const int VK_ESCAPE = 0x1B;
		private const int VK_SHIFT = 0x10;
		private const int VK_CONTROL = 0x11;
		private const int VK_MENU = 0x12;
		private const int VK_LWIN = 0x5B;
		private const int VK_RWIN = 0x5C;

		private readonly LowLevelKeyboardProc _keyboardProc;
		private readonly LowLevelMouseProc _mouseProc;
		private IntPtr _keyboardHookId = IntPtr.Zero;
		private IntPtr _mouseHookId = IntPtr.Zero;
		private bool _isCapturing;
		private bool _isDisposed;

		public InputBindingCapture()
		{
			this._keyboardProc = this.KeyboardHookCallback;
			this._mouseProc = this.MouseHookCallback;
		}

		public event EventHandler<string> Captured;

		public event EventHandler CaptureCancelled;

		public bool IsCapturing => this._isCapturing;

		public void Start()
		{
			if (this._isDisposed || this._isCapturing)
			{
				return;
			}

			using (Process currentProcess = Process.GetCurrentProcess())
			using (ProcessModule currentModule = currentProcess.MainModule)
			{
				IntPtr moduleHandle = InputBindingCaptureNativeMethods.GetModuleHandle(currentModule.ModuleName);
				this._keyboardHookId = InputBindingCaptureNativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, this._keyboardProc, moduleHandle, 0);
				this._mouseHookId = InputBindingCaptureNativeMethods.SetWindowsHookEx(WH_MOUSE_LL, this._mouseProc, moduleHandle, 0);
			}

			this._isCapturing = this._keyboardHookId != IntPtr.Zero && this._mouseHookId != IntPtr.Zero;
		}

		public void Stop()
		{
			if (this._keyboardHookId != IntPtr.Zero)
			{
				InputBindingCaptureNativeMethods.UnhookWindowsHookEx(this._keyboardHookId);
				this._keyboardHookId = IntPtr.Zero;
			}

			if (this._mouseHookId != IntPtr.Zero)
			{
				InputBindingCaptureNativeMethods.UnhookWindowsHookEx(this._mouseHookId);
				this._mouseHookId = IntPtr.Zero;
			}

			this._isCapturing = false;
		}

		public void Dispose()
		{
			if (this._isDisposed)
			{
				return;
			}

			this._isDisposed = true;
			this.Stop();
			GC.SuppressFinalize(this);
		}

		private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && this._isCapturing && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
			{
				KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
				int virtualKey = hookStruct.vkCode;

				if (virtualKey == VK_ESCAPE)
				{
					this.Stop();
					this.CaptureCancelled?.Invoke(this, EventArgs.Empty);
					return (IntPtr)1;
				}

				if (IsModifierKey(virtualKey))
				{
					return InputBindingCaptureNativeMethods.CallNextHookEx(this._keyboardHookId, nCode, wParam, lParam);
				}

				Keys keyData = BuildKeyData(virtualKey);
				if (keyData != Keys.None)
				{
					this.CompleteCapture(InputBindingHelper.FormatKeyboardBinding(keyData));
					return (IntPtr)1;
				}
			}

			return InputBindingCaptureNativeMethods.CallNextHookEx(this._keyboardHookId, nCode, wParam, lParam);
		}

		private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && this._isCapturing)
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
					this.CompleteCapture(InputBindingHelper.FormatMouseBinding(button.Value));
					return (IntPtr)1;
				}
			}

			return InputBindingCaptureNativeMethods.CallNextHookEx(this._mouseHookId, nCode, wParam, lParam);
		}

		private void CompleteCapture(string binding)
		{
			this.Stop();
			this.Captured?.Invoke(this, binding);
		}

		private static bool IsModifierKey(int virtualKey)
		{
			return virtualKey == VK_SHIFT
				|| virtualKey == VK_CONTROL
				|| virtualKey == VK_MENU
				|| virtualKey == VK_LWIN
				|| virtualKey == VK_RWIN
				|| virtualKey == 0xA0 // Left shift
				|| virtualKey == 0xA1 // Right shift
				|| virtualKey == 0xA2 // Left control
				|| virtualKey == 0xA3 // Right control
				|| virtualKey == 0xA4 // Left alt
				|| virtualKey == 0xA5; // Right alt
		}

		private static Keys BuildKeyData(int virtualKey)
		{
			Keys key = (Keys)virtualKey;
			Keys modifiers = Keys.None;

			if (InputBindingCaptureNativeMethods.GetAsyncKeyState(VK_CONTROL) < 0)
			{
				modifiers |= Keys.Control;
			}

			if (InputBindingCaptureNativeMethods.GetAsyncKeyState(VK_SHIFT) < 0)
			{
				modifiers |= Keys.Shift;
			}

			if (InputBindingCaptureNativeMethods.GetAsyncKeyState(VK_MENU) < 0)
			{
				modifiers |= Keys.Alt;
			}

			return modifiers | key;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct KBDLLHOOKSTRUCT
		{
			public int vkCode;
			public int scanCode;
			public int flags;
			public int time;
			public IntPtr dwExtraInfo;
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

	static class InputBindingCaptureNativeMethods
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int vKey);
	}
}

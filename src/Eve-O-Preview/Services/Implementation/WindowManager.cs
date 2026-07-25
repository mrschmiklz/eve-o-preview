using System;
using System.Drawing;
using System.Runtime.InteropServices;
using EveOPreview.Configuration;
using EveOPreview.Services.Interop;

namespace EveOPreview.Services.Implementation
{
	public class WindowManager : IWindowManager
	{
		#region Private constants
		private const int WINDOW_SIZE_THRESHOLD = 300;
		private const int NO_ANIMATION = 0;
		#endregion

		private const string EXCEPTION_DUMP_FILE_NAME = "EVE-O-Preview.log";

		private int? _currentAnimationSetting = null;
		private ANIMATIONINFO _animationParam = new ANIMATIONINFO();

		public WindowManager(IThumbnailConfiguration configuration)
		{
			this.IsCompositionEnabled =
				((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor >= 2))
				|| (Environment.OSVersion.Version.Major >= 10)
				|| DwmNativeMethods.DwmIsCompositionEnabled();
			_animationParam.cbSize = (UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO));
		}

		public bool IsCompositionEnabled { get; }

		public IntPtr GetForegroundWindowHandle()
		{
			return User32NativeMethods.GetForegroundWindow();
		}

		public void TurnOffAnimation()
		{
			var currentAnimationSetup = User32NativeMethods.SystemParametersInfo(User32NativeMethods.SPI_GETANIMATION, (Int32)Marshal.SizeOf(typeof(ANIMATIONINFO)), ref _animationParam, 0);
			if (_currentAnimationSetting == null)
			{
				_currentAnimationSetting = _animationParam.iMinAnimate;
			}

			if (currentAnimationSetup != NO_ANIMATION)
			{
				_animationParam.iMinAnimate = NO_ANIMATION;
				User32NativeMethods.SystemParametersInfo(User32NativeMethods.SPI_SETANIMATION, (Int32)Marshal.SizeOf(typeof(ANIMATIONINFO)), ref _animationParam, 0);
			}
		}

		public void RestoreAnimation()
		{
			User32NativeMethods.SystemParametersInfo(User32NativeMethods.SPI_GETANIMATION, (Int32)Marshal.SizeOf(typeof(ANIMATIONINFO)), ref _animationParam, 0);
			if (_animationParam.iMinAnimate != (int)_currentAnimationSetting)
			{
				_animationParam.iMinAnimate = (int)_currentAnimationSetting;
				User32NativeMethods.SystemParametersInfo(User32NativeMethods.SPI_SETANIMATION, (Int32)Marshal.SizeOf(typeof(ANIMATIONINFO)), ref _animationParam, 0);
			}
		}

		public bool ActivateWindow(IntPtr handle, AnimationStyle animation)
		{
			if (handle == IntPtr.Zero)
			{
				return false;
			}

			this.RestoreWindowIfMinimized(handle, animation);
			this.ForceForegroundWindow(handle);

			if (User32NativeMethods.GetForegroundWindow() == handle)
			{
				return true;
			}

			return !User32NativeMethods.IsIconic(handle);
		}

		private void RestoreWindowIfMinimized(IntPtr handle, AnimationStyle animation)
		{
			if (!User32NativeMethods.IsIconic(handle))
			{
				return;
			}

			switch (animation)
			{
				case AnimationStyle.OriginalAnimation:
					User32NativeMethods.ShowWindowAsync(handle, InteropConstants.SW_RESTORE);
					break;
				case AnimationStyle.NoAnimation:
					this.TurnOffAnimation();
					User32NativeMethods.ShowWindowAsync(handle, InteropConstants.SW_RESTORE);
					this.RestoreAnimation();
					break;
			}
		}

		private void ForceForegroundWindow(IntPtr handle)
		{
			IntPtr foregroundWindow = User32NativeMethods.GetForegroundWindow();
			uint foregroundThread = User32NativeMethods.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
			uint targetThread = User32NativeMethods.GetWindowThreadProcessId(handle, IntPtr.Zero);
			uint currentThread = User32NativeMethods.GetCurrentThreadId();
			bool attachedToForeground = false;
			bool attachedToTarget = false;

			try
			{
				if (foregroundThread != 0 && foregroundThread != currentThread)
				{
					attachedToForeground = User32NativeMethods.AttachThreadInput(currentThread, foregroundThread, true);
				}

				if (targetThread != 0 && targetThread != currentThread)
				{
					attachedToTarget = User32NativeMethods.AttachThreadInput(currentThread, targetThread, true);
				}

				User32NativeMethods.BringWindowToTop(handle);
				User32NativeMethods.SetForegroundWindow(handle);
				User32NativeMethods.SetFocus(handle);
			}
			finally
			{
				if (attachedToTarget)
				{
					User32NativeMethods.AttachThreadInput(currentThread, targetThread, false);
				}

				if (attachedToForeground)
				{
					User32NativeMethods.AttachThreadInput(currentThread, foregroundThread, false);
				}
			}
		}

		public void MinimizeWindow(IntPtr handle, AnimationStyle animation, bool enableAnimation)
		{
			if (enableAnimation)
			{
				switch (animation)
				{
					case AnimationStyle.OriginalAnimation:
						User32NativeMethods.SendMessage(handle, InteropConstants.WM_SYSCOMMAND, InteropConstants.SC_MINIMIZE, 0);
						break;
					case AnimationStyle.NoAnimation:
						this.TurnOffAnimation();
						User32NativeMethods.SendMessage(handle, InteropConstants.WM_SYSCOMMAND, InteropConstants.SC_MINIMIZE, 0);
						this.RestoreAnimation();
						break;
				}
			}
			else
			{
				switch (animation)
				{
					case AnimationStyle.OriginalAnimation:
						WINDOWPLACEMENT param = new WINDOWPLACEMENT();
						param.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
						User32NativeMethods.GetWindowPlacement(handle, ref param);
						param.showCmd = WINDOWPLACEMENT.SW_MINIMIZE;
						User32NativeMethods.SetWindowPlacement(handle, ref param);
						break;
					case AnimationStyle.NoAnimation:
						this.TurnOffAnimation();
						User32NativeMethods.SendMessage(handle, InteropConstants.WM_SYSCOMMAND, InteropConstants.SC_MINIMIZE, 0);
						this.RestoreAnimation();
						break;
				}
			}
		}

		public void MoveWindow(IntPtr handle, int left, int top, int width, int height)
		{
			User32NativeMethods.MoveWindow(handle, left, top, width, height, true);
		}

		public void MaximizeWindow(IntPtr handle)
		{
			User32NativeMethods.ShowWindowAsync(handle, InteropConstants.SW_SHOWMAXIMIZED);
		}

		public (int Left, int Top, int Right, int Bottom) GetWindowPosition(IntPtr handle)
		{
			User32NativeMethods.GetWindowRect(handle, out RECT windowRectangle);

			return (windowRectangle.Left, windowRectangle.Top, windowRectangle.Right, windowRectangle.Bottom);
		}

		public Size GetClientAreaSize(IntPtr handle)
		{
			if (handle == IntPtr.Zero || !User32NativeMethods.GetClientRect(handle, out RECT clientRectangle))
			{
				return Size.Empty;
			}

			return new Size(
				clientRectangle.Right - clientRectangle.Left,
				clientRectangle.Bottom - clientRectangle.Top);
		}

		public bool IsWindowMaximized(IntPtr handle)
		{
			return User32NativeMethods.IsZoomed(handle);
		}

		public bool IsWindowMinimized(IntPtr handle)
		{
			return User32NativeMethods.IsIconic(handle);
		}

		public IDwmThumbnail GetLiveThumbnail(IntPtr destination, IntPtr source)
		{
			IDwmThumbnail thumbnail = new DwmThumbnail(this);
			thumbnail.Register(destination, source);

			return thumbnail;
		}

		public Image GetStaticThumbnail(IntPtr source)
		{
			var sourceContext = User32NativeMethods.GetDC(source);

			User32NativeMethods.GetClientRect(source, out RECT windowRect);

			var width = windowRect.Right - windowRect.Left;
			var height = windowRect.Bottom - windowRect.Top;

			if ((width < WINDOW_SIZE_THRESHOLD) || (height < WINDOW_SIZE_THRESHOLD))
			{
				User32NativeMethods.ReleaseDC(source, sourceContext);

				return null;
			}

			var destContext = Gdi32NativeMethods.CreateCompatibleDC(sourceContext);
			var bitmap = Gdi32NativeMethods.CreateCompatibleBitmap(sourceContext, width, height);

			var oldBitmap = Gdi32NativeMethods.SelectObject(destContext, bitmap);
			Gdi32NativeMethods.BitBlt(destContext, 0, 0, width, height, sourceContext, 0, 0, Gdi32NativeMethods.SRCCOPY);
			Gdi32NativeMethods.SelectObject(destContext, oldBitmap);
			Gdi32NativeMethods.DeleteDC(destContext);
			User32NativeMethods.ReleaseDC(source, sourceContext);

			Image image = Image.FromHbitmap(bitmap);
			Gdi32NativeMethods.DeleteObject(bitmap);

			return image;
		}
	}
}

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace EveOPreview
{
	// A really very primitive exception handler stuff here
	// No IoC, no fancy DI containers - just a plain exception stacktrace dump
	// If this code is called then something was gone really bad
	// so even the DI infrastructure might be dead already.
	// So this dumb and non elegant approach is used
	sealed class ExceptionHandler
	{
		private const string EXCEPTION_DUMP_FILE_NAME = "EVE-O-Preview.log";

		private const string EXCEPTION_MESSAGE = "EVE-O-Preview has encountered a problem and needs to close. Additional information has been saved in the crash log file.";

		public void SetupExceptionHandlers()
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				return;
			}

			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += delegate (Object sender, ThreadExceptionEventArgs e)
			{
				this.ExceptionEventHandler(e.Exception);
			};

			AppDomain.CurrentDomain.UnhandledException += delegate (Object sender, UnhandledExceptionEventArgs e)
			{
				this.ExceptionEventHandler(e.ExceptionObject as Exception);
			};
		}

		private void ExceptionEventHandler(Exception exception)
		{
			try
			{
				if (exception != null)
				{
					string logPath = Path.Combine(this.GetLogDirectory(), ExceptionHandler.EXCEPTION_DUMP_FILE_NAME);
					File.WriteAllText(logPath, exception.ToString());
				}

				MessageBox.Show(ExceptionHandler.EXCEPTION_MESSAGE, @"EVE-O-Preview", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch
			{
				// We are in unstable state now so even this operation might fail
				// Still we actually don't care anymore - anyway the application has been cashed
			}

			System.Environment.Exit(1);
		}

		private string GetLogDirectory()
		{
			try
			{
				string executablePath = Application.ExecutablePath;
				if (!string.IsNullOrWhiteSpace(executablePath))
				{
					return Path.GetDirectoryName(executablePath) ?? AppDomain.CurrentDomain.BaseDirectory;
				}
			}
			catch
			{
			}

			return AppDomain.CurrentDomain.BaseDirectory;
		}
	}
}
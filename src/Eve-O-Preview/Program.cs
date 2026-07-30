using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Presenters;
using EveOPreview.Services;
using EveOPreview.View;

namespace EveOPreview
{
	static class Program
	{
		private static string MUTEX_NAME = "EVE-O-Preview Single Instance Mutex";

		private static Mutex _singleInstanceMutex;

		/// <summary>The main entry point for the application.</summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool smokeTest = Program.IsSmokeTestRequested(args);

			Program._singleInstanceMutex = Program.GetInstanceToken();

			if (Program._singleInstanceMutex == null)
			{
				return;
			}

			ExceptionHandler handler = new ExceptionHandler();
			handler.SetupExceptionHandlers();

			IApplicationController controller = Program.InitializeApplicationController();

			Program.InitializeWinForms();

			if (smokeTest)
			{
				Program.ScheduleSmokeTestExit(TimeSpan.FromSeconds(6));
			}

			controller.Run<MainFormPresenter>();
		}

		private static bool IsSmokeTestRequested(string[] args)
		{
			if (args != null && args.Any(arg => string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}

			return string.Equals(Environment.GetEnvironmentVariable("EVEOPREVIEW_SMOKE_TEST"), "1", StringComparison.Ordinal);
		}

		private static void ScheduleSmokeTestExit(TimeSpan delay)
		{
			Task.Run(async () =>
			{
				await Task.Delay(delay);
				try
				{
					Application.Exit();
				}
				catch
				{
					Environment.Exit(0);
				}
			});
		}

		private static Mutex GetInstanceToken()
		{
			try
			{
				Mutex.OpenExisting(Program.MUTEX_NAME);
				return null;
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
			catch (Exception)
			{
				Mutex token = new Mutex(true, Program.MUTEX_NAME, out var result);
				return result ? token : null;
			}
		}

		private static void InitializeWinForms()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		}

		private static IApplicationController InitializeApplicationController()
		{
			IIocContainer container = new LightInjectContainer();

			container.Register<IWindowManager>();
			container.Register<IProcessMonitor>();

			container.Register<IConfigurationStorage>();
			container.Register<IAppConfig>();
			container.Register<IThumbnailConfiguration>();

			container.Register<IThumbnailManager>();
			container.Register<IThumbnailViewFactory>();

			IApplicationController controller = new ApplicationController(container);

			controller.RegisterView<LiveThumbnailView, LiveThumbnailView>();

			controller.RegisterView<IMainFormView, MainForm>();
			controller.RegisterInstance(new ApplicationContext());

			return controller;
		}
	}
}

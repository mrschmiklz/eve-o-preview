using System;
using System.Threading;
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
		static void Main()
		{
			Program._singleInstanceMutex = Program.GetInstanceToken();

			if (Program._singleInstanceMutex == null)
			{
				return;
			}

			ExceptionHandler handler = new ExceptionHandler();
			handler.SetupExceptionHandlers();

			IApplicationController controller = Program.InitializeApplicationController();

			Program.InitializeWinForms();

			Application.SetCompatibleTextRenderingDefault(false);

			controller.Run<MainFormPresenter>();
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

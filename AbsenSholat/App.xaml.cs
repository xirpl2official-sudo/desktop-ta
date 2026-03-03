using System.Windows;
using AbsenSholat.Services;

namespace AbsenSholat
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize debug console
            Logger.Initialize();
            Logger.Info("App", "Application starting...");
            Logger.Info("App", $"Version: 1.0.0");
            Logger.Info("App", $"Start Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("App", "Application shutting down...");
            Logger.Shutdown();
            base.OnExit(e);
        }
    }
}

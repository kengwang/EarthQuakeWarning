using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using EarthquakeWaring.App.Extensions;
using EarthquakeWaring.App.Infrastructure.Models.BaseModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.Models.ViewModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Pages;
using EarthquakeWaring.App.Services;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EarthquakeWaring.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly IHost Host;

        public static bool MainWindowOpened = false;
        public static Frame? RootFrame = null;

        public App()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
            var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder();
            builder.ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);
            Host = builder.Build();
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddSerilog(new LoggerConfiguration()
#if DEBUG
                .WriteTo.Console()
#endif
                .WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day)
                .CreateLogger());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DI.Services = Host.Services;

            // Kill Other Instance
            foreach (var process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)
                         .Where(t => t.Id != Environment.ProcessId))
            {
                process.Kill(true);
            }


            // Check if want GUI
            if (!e.Args.Contains("/nogui"))
            {
                Host.Services.GetService<MainWindow>()?.Show();
            }

            if (Host.Services.GetService<ISetting<UpdaterSetting>>()?.Setting?.ShowNotifyIcon is not false)
                Host.Services.GetService<ITrayIconHolder>()?.ShowIcon();
            Host.RunAsync();
        }

        public static void ConfigureServices(IServiceCollection service)
        {
            // For Background Services
            service.AddSingleton<IEarthQuakeCalculator, HuaniaEarthQuakeCalculator>();
            service.AddSingleton<IEarthQuakeApi, HuaniaEarthQuakeApi>();
            service.AddSingleton<IHttpRequester, HttpRequester>();
            service.AddTransient<IEarthQuakeTracker, EarthQuakeTracker>();
            service.AddSingleton<INotificationPublisher, NotificationPublisher>();
            service.AddSingleton<INotificationHandler<HeartBeatNotification>, EarthQuakeInfoUpdater>();
            service.AddSingleton<IJsonConvertService, JsonConvertService>();
            service.AddSingleton(typeof(ISetting<>), typeof(FileJsonSetting<>));

            // For UI
            service.AddTransient<MainWindow>();
            service.AddTransient<SettingsPage>();
            service.AddTransient<EarthQuakesListPage>();
            service.AddTransient<EarthQuakeDetail>();
            service.AddTransient<SettingsPageViewModel>();
            service.AddTransient<EarthQuakeExamplesPage>();

            service.AddSingleton<ITrayIconHolder, TrayIconHolder>();

            service.AddHostedService<HeartBeatBackgroundService>();
        }
    }
}
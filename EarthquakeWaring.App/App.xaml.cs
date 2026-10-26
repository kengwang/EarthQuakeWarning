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
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using EarthquakeWaring.App.Services.EarthQuakeApis;
using EarthquakeWaring.App.Services.EarthQuakeSources;

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
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!);
            var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder();
            builder.ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);
            Host = builder.Build();
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.ClearProviders();
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
            Host.Services.GetService<INTPHandler>()?.GetNTPServerTime();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            Host.Services.GetService<IGNSSHandler>()?.GetCurrentInfoAsync(cts.Token);
            Host.RunAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            }
            finally
            {
                Host.Dispose();
                base.OnExit(e);
            }
        }

        public static void ConfigureServices(IServiceCollection service)
        {
            // For Background Services
            service.AddSingleton<IEarthQuakeCalculator, HuaniaEarthQuakeCalculator>();
            service.AddSingleton<HuaniaEarthQuakeApi>();
            service.AddSingleton<SichuanEarthQuakeApi>();
            service.AddSingleton<WolfxSceewApi>();
            service.AddSingleton<WolfxCencApi>();
            service.AddSingleton<MiuiEarthQuakeApi>();
            service.AddSingleton<CencEarthQuakeApi>();
            service.AddSingleton<CencRapidEarthQuakeApi>();
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<HuaniaEarthQuakeApi>());
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<SichuanEarthQuakeApi>());
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<WolfxSceewApi>());
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<WolfxCencApi>());
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<MiuiEarthQuakeApi>());
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<CencEarthQuakeApi>());
            service.AddSingleton<IEarthQuakeApi>(provider => provider.GetRequiredService<CencRapidEarthQuakeApi>());
            service.AddHttpClient("Wolfx", client =>
            {
                client.BaseAddress = new Uri("https://api.wolfx.jp/");
                client.Timeout = TimeSpan.FromSeconds(10);
            });
            service.AddSingleton<WolfxMessageParser>();
            service.AddSingleton<IWolfxReportCache, WolfxReportCache>();
            service.AddSingleton<WolfxHttpQuerySource>();
            service.AddSingleton<WolfxWebSocketRealtimeSource>();
            service.AddSingleton<WolfxWebSocketApi>();
            service.AddSingleton<IEarthquakeSourceCatalog, EarthquakeSourceCatalog>();
            service.AddSingleton<IHttpRequester, HttpRequester>();
            service.AddTransient<IEarthQuakeTracker, EarthQuakeTracker>();
            service.AddSingleton<INotificationPublisher, NotificationPublisher>();
            service.AddSingleton<EarthQuakeInfoUpdater>();
            service.AddSingleton<IEarthQuakeInfoUpdater>(provider => provider.GetRequiredService<EarthQuakeInfoUpdater>());
            service.AddSingleton<IJsonConvertService, JsonConvertService>();
            service.AddSingleton(typeof(ISetting<>), typeof(FileJsonSetting<>));
            service.AddSingleton<ITimeHandler, TimeManager>();
            service.AddSingleton<INTPHandler, NTPTimeManager>();
            service.AddSingleton<IGNSSHandler, GNSSManager>();

            // For UI
            service.AddTransient<MainWindow>();
            service.AddTransient<SettingsPage>();
            service.AddTransient<EarthQuakesListPage>();
            service.AddTransient<EarthQuakeDetail>();
            service.AddTransient<SettingsPageViewModel>();
            service.AddTransient<EarthQuakeExamplesPage>();

            service.AddSingleton<ITrayIconHolder, TrayIconHolder>();

            service.AddHostedService(provider => provider.GetRequiredService<EarthQuakeInfoUpdater>());
            service.AddSingleton<IVolumeManager, VolumeManager>();
        }
    }
}

using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.Models.ViewModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using GuerrillaNtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace EarthquakeWaring.App.Pages;

public partial class SettingsPage : Page
{
    private bool dontFire = false;
    private readonly IServiceProvider _services;
    private readonly IHostApplicationLifetime _lifetime;

    public SettingsPage(SettingsPageViewModel vm, IServiceProvider services, IHostApplicationLifetime lifetime)
    {
        _services = services;
        _lifetime = lifetime;
        InitializeComponent();
        DataContext = vm;
        dontFire = true;
        StartupSwitch.IsChecked =
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\")?.GetValue(nameof(EarthquakeWaring)) != null;
        dontFire = false;
    }

    private void OpenPositionSelector(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://api.map.baidu.com/lbsapi/getpoint/");
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (dontFire) return;
        Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run")
            .SetValue(nameof(EarthquakeWaring), Environment.ProcessPath + " /nogui");
    }

    private void StartupSwitch_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (dontFire) return;
        Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run")
            .DeleteValue(nameof(EarthquakeWaring));
    }

    private void DeveloperClicked(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://github.com/kengwang");
    }

    private void OpenSourceClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://github.com/kengwang/EarthQuakeWarning");
    }

    private void ThanksClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "http://www.365icl.com/");
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        _lifetime.StopApplication();
        Application.Current.Shutdown();
    }
    public async void TestNTPServer(object sender, RoutedEventArgs e)
    {
        var setting = _services.GetService<ISetting<TimeSetting>>();
        var server = setting?.Setting?.NTPServer;
        var client = new NtpClient(server, TimeSpan.FromMilliseconds(500));
        try
        {
            var result = await client.QueryAsync();
            if (result.Synchronized)
            {
                MessageBox.Show("NTP服务器状态正常");
            }
            else
            {
                MessageBox.Show("NTP服务器状态异常");
            }
        }
        catch
        {
            MessageBox.Show("NTP服务器状态异常");
        }
    }
}
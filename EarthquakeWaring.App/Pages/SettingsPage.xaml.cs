using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.ViewModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;

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
        dontFire= true;
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
}
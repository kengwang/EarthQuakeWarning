using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.ViewModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EarthquakeWaring.App.Pages;

public partial class SettingsPage : Page
{
    private bool dontFire = false;
    private readonly IServiceProvider _services;

    public SettingsPage(SettingsPageViewModel vm, IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
        DataContext = vm;
        dontFire= true;
        StartupSwitch.IsChecked =
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\")?.GetValue(nameof(EarthquakeWaring)) != null;
        dontFire = false;
    }

    private void OpenPositionSelector(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://lbs.amap.com/tools/picker");
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
}
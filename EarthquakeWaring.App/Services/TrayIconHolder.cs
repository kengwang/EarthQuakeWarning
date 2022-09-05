using System;
using System.Windows.Forms;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace EarthquakeWaring.App.Services;

public class TrayIconHolder : ITrayIconHolder, IDisposable
{
    private readonly IServiceProvider _sp;
    private readonly NotifyIcon _notifyIcon;
    private bool _mainWindowOpened;

    public TrayIconHolder(IServiceProvider sp)
    {
        _sp = sp;
        _notifyIcon = new System.Windows.Forms.NotifyIcon();
        _notifyIcon.Icon = new System.Drawing.Icon(@"Icons\radio.ico");
        _notifyIcon.Click += NotifyIconOnClick;
    }

    private void NotifyIconOnClick(object? sender, EventArgs e)
    {
        if (_mainWindowOpened) return;
        _mainWindowOpened = true;
        _sp.GetService<MainWindow>()?.ShowDialog();
        _mainWindowOpened = false;
    }

    public void ShowIcon()
    {
        _notifyIcon.Visible = true;
    }

    public void HideIcon()
    {
        _notifyIcon.Visible = false;
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}
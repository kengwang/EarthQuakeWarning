using EarthquakeWaring.App.Icons;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows.Forms;

namespace EarthquakeWaring.App.Services;

public class TrayIconHolder : ITrayIconHolder, IDisposable
{
    private readonly IServiceProvider _sp;
    private readonly NotifyIcon _notifyIcon;

    public TrayIconHolder(IServiceProvider sp)
    {
        _sp = sp;
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Text = "地震预警 正在运行";
        Directory.CreateDirectory("Icons");
        if (!File.Exists(@"Icons\Original.ico"))
        {
            var imageOriginal = Convert.FromBase64String(IconResource.Original);
            File.WriteAllBytes(@"Icons\original.ico", imageOriginal);
        }
        _notifyIcon.Icon = new System.Drawing.Icon(@"Icons\original.ico");
        _notifyIcon.Click += NotifyIconOnClick;
    }

    private void NotifyIconOnClick(object? sender, EventArgs e)
    {
        if (App.MainWindowOpened) return;
        _sp.GetService<MainWindow>()?.Show();
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
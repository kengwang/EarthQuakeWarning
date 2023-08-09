using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using Microsoft.Web.WebView2.Core;
using System;
using System.Windows.Controls;

namespace EarthquakeWaring.App.Pages;

public partial class EarthQuakeDetail : Page
{
    public EarthQuakeDetail(EarthQuakeTrackingInformation info, CurrentPosition? currentPosition)
    {
        DataContext = info;
        InitializeComponent();
        var url = $"https://uri.amap.com/marker?markers={info.Longitude},{info.Latitude},震中：{info.Position}";
        if (currentPosition != null)
        {
            url += $"|{currentPosition.Longitude},{currentPosition.Latitude},我的位置";
        }

        MapView.Source = new Uri(url);
    }

    private void MapView_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        MapView.ExecuteScriptAsync(@"
//document.getElementById('search').style.display = 'none'
document.getElementById('loginbox').style.display = 'none'
document.getElementById('amapAppDownload').style.display = 'none'
//document.getElementById('layerbox_item').style.display = 'none'
document.getElementsByClassName('satellite')[0].click()
document.getElementById('amap-result-banner').style.display = 'none'
");
    }
}
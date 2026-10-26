using EarthquakeWaring.App.Extensions;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.Models.ViewModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Services;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Button = Wpf.Ui.Controls.Button;

namespace EarthquakeWaring.App.Pages;

public partial class EarthQuakesListPage : Page
{
    private readonly ISetting<CurrentPosition> _currentPosition;
    private readonly ISetting<AlertLimit> _alertLimit;
    private readonly IEarthquakeSourceCatalog _sourceCatalog;
    private readonly ISetting<UpdaterSetting> _updaterSetting;
    private readonly IEarthQuakeCalculator _calculator;
    private readonly IServiceProvider _service;
    private readonly EarthQuakesListPageViewModel _viewModel;


    public EarthQuakesListPage()
    {
        _service = DI.Services;
        _calculator = _service.GetRequiredService<IEarthQuakeCalculator>();
        _sourceCatalog = _service.GetRequiredService<IEarthquakeSourceCatalog>();
        _updaterSetting = _service.GetRequiredService<ISetting<UpdaterSetting>>();
        _alertLimit = _service.GetRequiredService<ISetting<AlertLimit>>();
        _currentPosition = _service.GetRequiredService<ISetting<CurrentPosition>>();

        _viewModel = new EarthQuakesListPageViewModel();
        InitializeComponent();
    }

    protected override async void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        _viewModel.ProgressRingVisibility = Visibility.Visible;
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var querySource = _sourceCatalog.GetQuerySource(
            _updaterSetting.Setting?.SourceId ?? EarthquakeSourceIds.Huania);
        var results = await querySource.GetRecentAsync(cancellationToken);
        foreach (var result in results)
            _viewModel.InformationList.Add(ConvertToInformation(result));
        ListView.ItemsSource = _viewModel.InformationList;
        _viewModel.ProgressRingVisibility = Visibility.Collapsed;
        LoadingRing.Visibility = Visibility.Collapsed;
    }

    private EarthQuakeTrackingInformation ConvertToInformation(EarthquakeReport latestInfo)
    {
        var trackingInformation = new EarthQuakeTrackingInformation
        {
            // Update the tracking information
            Provider = latestInfo.EventKey.Provider,
            Position = latestInfo.Hypocenter ?? "未知地点",
            StartTime = latestInfo.OriginTime.LocalDateTime,
            UpdateTime = latestInfo.ReportTime.LocalDateTime,
            Depth = latestInfo.Depth ?? 0,
            Latitude = latestInfo.Latitude,
            Longitude = latestInfo.Longitude,
            Id = latestInfo.EventKey.EventId,
            Magnitude = latestInfo.Magnitude
        };

        if (_currentPosition.Setting == null) return trackingInformation;
        trackingInformation.Distance = _calculator.GetDistance(_currentPosition.Setting.Latitude,
            _currentPosition.Setting.Longitude, trackingInformation.Latitude, trackingInformation.Longitude);
        trackingInformation.TheoryCountDown =
            (int)_calculator.GetCountDownSeconds(trackingInformation.Depth,
                trackingInformation.Distance);
        trackingInformation.Intensity =
            _calculator.GetIntensity(trackingInformation.Magnitude, trackingInformation.Distance);
        trackingInformation.Stage = EarthQuakeTracker.GetEarthQuakeAlertStage(trackingInformation);
        trackingInformation.DontUseShouldAlert =
            EarthQuakeTracker.ShouldPopupAlert(trackingInformation, _alertLimit.Setting ?? new AlertLimit());
        return trackingInformation;
    }


    private async void SimulateEarthQuake(object sender, RoutedEventArgs e)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var info = (((sender as Button)?.Tag as EarthQuakeTrackingInformation)!);
        var tracker = _service.GetService<IEarthQuakeTracker>();
        var timeHandler = _service.GetService<ITimeHandler>();
        var querySource = _sourceCatalog.GetQuerySource(
            _updaterSetting.Setting?.SourceId ?? EarthquakeSourceIds.Huania);
        var reports = await querySource.GetEventReportsAsync(
            new EarthquakeEventKey(info.Provider, info.Id), cancellationTokenSource.Token);
        if (tracker is null || reports.Count == 0)
        {
            cancellationTokenSource.Dispose();
            return;
        }

        tracker.SimulateTimeSpan = DateTime.Now + timeHandler!.Offset - info.StartTime;
        tracker.SimulateReports = reports;
        var channel = Channel.CreateUnbounded<EarthquakeReport>();
        channel.Writer.TryComplete();
        _ = tracker.TrackAsync(reports[0], channel.Reader, cancellationTokenSource.Token)
            .ContinueWith(_ => cancellationTokenSource.Dispose(), TaskScheduler.Default);
    }

    private void ShowEarthQuakeDetail(object sender, MouseButtonEventArgs e)
    {
        if (((Grid)sender).Tag is EarthQuakeTrackingInformation info)
        {
            // _service.GetService<MainWindow>()?.RootFrame.Navigate(new EarthQuakeDetail(info, _currentPosition.Setting));
            if (!App.MainWindowOpened)
            {
                _service.GetService<MainWindow>()?.Show();
            }

            if (App.MainWindowOpened)
            {
                App.RootFrame?.Navigate(new EarthQuakeDetail(info, _currentPosition.Setting));
            }
        }
    }
}

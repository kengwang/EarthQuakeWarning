using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services;

public class EarthQuakeTracker : IEarthQuakeTracker
{
    private readonly IEarthQuakeCalculator _earthQuakeCalculator;
    private readonly ISetting<CurrentPosition> _currentPosition;
    private readonly ISetting<AlertLimit> _alertLimit;
    private readonly ISetting<TrackerSetting> _trackerSetting;
    private readonly ITimeHandler _timeHandler;
    private readonly ILogger<EarthQuakeTracker> _logger;
    private readonly IServiceProvider _services;
    private readonly EarthQuakeTrackingInformation _trackingInformation = new();
    private EarlyWarningWindow? _warningWindow;

    public EarthQuakeTracker(
        IEarthQuakeCalculator earthQuakeCalculator,
        ISetting<CurrentPosition> currentPosition,
        ILogger<EarthQuakeTracker> logger,
        ISetting<AlertLimit> alertLimit,
        IServiceProvider services,
        ISetting<TrackerSetting> trackerSetting,
        ITimeHandler timeHandler)
    {
        _earthQuakeCalculator = earthQuakeCalculator ?? throw new ArgumentNullException(nameof(earthQuakeCalculator));
        _currentPosition = currentPosition ?? throw new ArgumentNullException(nameof(currentPosition));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _alertLimit = alertLimit ?? throw new ArgumentNullException(nameof(alertLimit));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _trackerSetting = trackerSetting ?? throw new ArgumentNullException(nameof(trackerSetting));
        _timeHandler = timeHandler ?? throw new ArgumentNullException(nameof(timeHandler));
    }

    public TimeSpan SimulateTimeSpan { get; set; } = TimeSpan.Zero;
    public IReadOnlyList<EarthquakeReport>? SimulateReports { get; set; }

    public async Task TrackAsync(
        EarthquakeReport initialReport,
        ChannelReader<EarthquakeReport> updates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(initialReport);
        ArgumentNullException.ThrowIfNull(updates);
        var current = initialReport;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (updates.TryRead(out var update))
                {
                    if (update.Status == EarthquakeReportStatus.Cancelled)
                    {
                        _logger.LogInformation("Earthquake {Provider}/{EventId} was cancelled",
                            update.EventKey.Provider, update.EventKey.EventId);
                        return;
                    }

                    current = update;
                }

                current = SelectSimulationReport(current);
                if (!TryBuildTrackingInformation(current, out var elapsedSeconds))
                    return;

                UpdateWindow(current, cancellationToken);
                if (elapsedSeconds > _trackingInformation.TheoryCountDown + 30)
                {
                    _logger.LogInformation("Earthquake {Provider}/{EventId} tracking expired",
                        current.EventKey.Provider, current.EventKey.EventId);
                    return;
                }

                var delay = Math.Max(5, _trackerSetting.Setting?.TrackerTimeSpanMillisecond ?? 5) * 100;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal tracker shutdown.
        }
    }

    private EarthquakeReport SelectSimulationReport(EarthquakeReport current)
    {
        if (SimulateReports is not { Count: > 0 })
            return current;
        var virtualNow = DateTimeOffset.Now + _timeHandler.Offset - SimulateTimeSpan;
        return SimulateReports.LastOrDefault(report => report.ReportTime <= virtualNow) ?? SimulateReports[0];
    }

    private bool TryBuildTrackingInformation(EarthquakeReport report, out double elapsedSeconds)
    {
        var now = DateTimeOffset.Now + _timeHandler.Offset - SimulateTimeSpan;
        elapsedSeconds = (now - report.OriginTime).TotalSeconds;
        if (SimulateReports is null && elapsedSeconds > TimeSpan.FromMinutes(5).TotalSeconds)
        {
            _logger.LogWarning("Ignoring expired earthquake {Provider}/{EventId}",
                report.EventKey.Provider, report.EventKey.EventId);
            return false;
        }

        var position = _currentPosition.Setting;
        if (position is null)
        {
            _logger.LogError("Current position is not set; earthquake distance cannot be calculated");
            return false;
        }

        var distance = _earthQuakeCalculator.GetDistance(position.Latitude, position.Longitude,
            report.Latitude, report.Longitude);
        var theory = (int)_earthQuakeCalculator.GetCountDownSeconds(report.Depth ?? 0, distance);
        var intensity = _earthQuakeCalculator.GetIntensity(report.Magnitude, distance);

        _trackingInformation.Provider = report.EventKey.Provider;
        _trackingInformation.Id = report.EventKey.EventId;
        _trackingInformation.Position = report.Hypocenter ?? "未知地点";
        _trackingInformation.StartTime = report.OriginTime.LocalDateTime;
        _trackingInformation.UpdateTime = report.ReportTime.LocalDateTime;
        _trackingInformation.Depth = report.Depth ?? 0;
        _trackingInformation.Latitude = report.Latitude;
        _trackingInformation.Longitude = report.Longitude;
        _trackingInformation.Magnitude = report.Magnitude;
        _trackingInformation.Distance = distance;
        _trackingInformation.TheoryCountDown = theory;
        _trackingInformation.Intensity = intensity;
        _trackingInformation.CountDown = (int)(theory - elapsedSeconds);
        _trackingInformation.Stage = GetEarthQuakeAlertStage(_trackingInformation);
        return true;
    }

    private void UpdateWindow(EarthquakeReport report, CancellationToken cancellationToken)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_warningWindow is not null)
                return;
            if (SimulateReports is null &&
                _alertLimit.Setting is { } limits &&
                !ShouldPopupAlert(_trackingInformation, limits))
                return;

            _logger.LogInformation(
                "Alert threshold reached for {Provider}/{EventId}; magnitude {Magnitude}, intensity {Intensity}",
                report.EventKey.Provider, report.EventKey.EventId, report.Magnitude, _trackingInformation.Intensity);
            _warningWindow = new EarlyWarningWindow(_trackingInformation, _services);
            _warningWindow.Show();
        }, DispatcherPriority.Normal, cancellationToken);
    }

    public static EarthQuakeStage GetEarthQuakeAlertStage(EarthQuakeTrackingInformation information) =>
        information.Intensity switch
        {
            >= 5 => EarthQuakeStage.Forced,
            >= 3 => EarthQuakeStage.Emergency,
            >= 1 => EarthQuakeStage.Warning,
            _ => EarthQuakeStage.Record
        };

    public static bool ShouldPopupAlert(EarthQuakeTrackingInformation information, AlertLimit alertLimit)
    {
        if (information.UpdateTime.Hour is >= 7 and <= 22)
            return information.Intensity >= alertLimit.DayIntensity && information.Magnitude >= alertLimit.DayMagnitude;
        return information.Intensity >= alertLimit.NightIntensity && information.Magnitude >= alertLimit.NightMagnitude;
    }
}

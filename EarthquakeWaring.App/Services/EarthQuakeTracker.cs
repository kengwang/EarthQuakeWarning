using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services;

public class EarthQuakeTracker : IEarthQuakeTracker
{
    private readonly IEarthQuakeApi _earthQuakeApi;
    private readonly IEarthQuakeCalculator _earthQuakeCalculator;
    private readonly ISetting<CurrentPosition> _currentPosition;
    private readonly ISetting<AlertLimit> _alertLimit;
    private readonly ILogger<EarthQuakeTracker> _logger;

    private EarlyWarningWindow? _warningWindow;
    private EarthQuakeTrackingInformation _trackingInformation = new();

    private CancellationTokenSource? _tokenSource;
    private CancellationToken _cancellationToken;

    public EarthQuakeTracker(IEarthQuakeApi earthQuakeApi, IEarthQuakeCalculator earthQuakeCalculator,
        ISetting<CurrentPosition> currentPosition, ILogger<EarthQuakeTracker> logger, ISetting<AlertLimit> alertLimit)
    {
        _earthQuakeApi = earthQuakeApi;
        _earthQuakeCalculator = earthQuakeCalculator;
        _currentPosition = currentPosition;
        _logger = logger;
        _alertLimit = alertLimit;
    }

    public TimeSpan SimulateTimeSpan { get; set; } = TimeSpan.Zero;

    public async Task StartTrack(HuaniaEarthQuake huaniaEarthQuake, CancellationTokenSource cancellationTokenSource)
    {
        _tokenSource = cancellationTokenSource;
        _cancellationToken = cancellationTokenSource.Token;
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            await CheckEarthQuake(huaniaEarthQuake).ConfigureAwait(false);
            await Task.Delay(500, _cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task CheckEarthQuake(HuaniaEarthQuake huaniaEarthQuake)
    {
        var infos = await _earthQuakeApi.GetEarthQuakeInfo(huaniaEarthQuake.EventId, _cancellationToken);
        EarthQuakeUpdate latestInfo;
        if (SimulateTimeSpan == TimeSpan.Zero)
        {
            latestInfo = infos[^1];
        }
        else
        {
            var simulatingInfo = infos.FirstOrDefault(t => t.UpdateAt > DateTime.Now - SimulateTimeSpan);
            if (infos.Count <= 0 && simulatingInfo == null) return;
            if (simulatingInfo == null) simulatingInfo = infos[^1];
            latestInfo = simulatingInfo;
        }

        if ((DateTime.Now - SimulateTimeSpan - latestInfo.UpdateAt).TotalSeconds >
            _trackingInformation.TheoryCountDown + 30)
        {
            _tokenSource?.Cancel(); // Expired Information
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Update the tracking information
            _trackingInformation.Position = latestInfo.Epicenter;
            _trackingInformation.StartTime = latestInfo.StartAt;
            _trackingInformation.UpdateTime = latestInfo.UpdateAt;
            _trackingInformation.Depth = latestInfo.Depth;
            _trackingInformation.Latitude = latestInfo.Latitude;
            _trackingInformation.Longitude = latestInfo.Longitude;
            _trackingInformation.EventId = latestInfo.EventId;
            _trackingInformation.Sations = latestInfo.Sations;
            _trackingInformation.Magnitude = latestInfo.Magnitude;


            if (_currentPosition.Setting != null)
            {
                _trackingInformation.Distance = _earthQuakeCalculator.GetDistance(_currentPosition.Setting.Latitude,
                    _currentPosition.Setting.Longitude, _trackingInformation.Latitude, _trackingInformation.Longitude);
                _trackingInformation.TheoryCountDown =
                    (int)_earthQuakeCalculator.GetCountDownSeconds(_trackingInformation.Depth,
                        _trackingInformation.Distance);
                _trackingInformation.Intensity =
                    _earthQuakeCalculator.GetIntensity(_trackingInformation.Magnitude, _trackingInformation.Distance);
                _trackingInformation.CountDown = (int)(_trackingInformation.TheoryCountDown -
                                                       (DateTime.Now - SimulateTimeSpan -
                                                        _trackingInformation.StartTime).TotalSeconds);
                _trackingInformation.Stage = GetEarthQuakeAlertStage();
                if (SimulateTimeSpan == TimeSpan.Zero || _warningWindow != null)
                    if (_trackingInformation.Stage < EarthQuakeStage.Warning || _warningWindow != null)
                        return;
                _warningWindow = new EarlyWarningWindow(_trackingInformation);
                _warningWindow.Show();
            }
            else
            {
                _logger.LogError("Current Position Not Set, cannot get the distance! Please set your position");
            }
        }, DispatcherPriority.Normal, _cancellationToken);
    }


    private EarthQuakeStage GetEarthQuakeAlertStage()
    {
        var w = 0;
        if (_trackingInformation.Intensity >= _alertLimit.Setting?.Intensity) w += 1;
        if (_trackingInformation.Magnitude >= _alertLimit.Setting?.Magnitude)
        {
            w += 1;
        }

        return (EarthQuakeStage)(_trackingInformation.Distance < 100 ? ++w : w);
    }
}
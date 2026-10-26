using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services;

/// <summary>Coordinates the selected real-time source and active per-event trackers.</summary>
public sealed class EarthQuakeInfoUpdater : BackgroundService, IEarthQuakeInfoUpdater
{
    private readonly IEarthquakeSourceCatalog _sources;
    private readonly IServiceProvider _services;
    private readonly ILogger<EarthQuakeInfoUpdater> _logger;
    private readonly ISetting<UpdaterSetting> _settings;
    private readonly ConcurrentDictionary<EarthquakeEventKey, ActiveTracker> _activeTrackers = new();

    /// <summary>Initializes the earthquake report coordinator.</summary>
    public EarthQuakeInfoUpdater(
        IEarthquakeSourceCatalog sources,
        IServiceProvider services,
        ILogger<EarthQuakeInfoUpdater> logger,
        ISetting<UpdaterSetting> settings)
    {
        _sources = sources ?? throw new ArgumentNullException(nameof(sources));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var setting = _settings.Setting ?? new UpdaterSetting();
                var selectedSource = setting.SourceId;
                using var sourceCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                PropertyChangedEventHandler onSettingChanged = (_, args) =>
                {
                    if (args.PropertyName is nameof(UpdaterSetting.SourceId) or nameof(UpdaterSetting.ApiType))
                        sourceCancellation.Cancel();
                };
                setting.PropertyChanged += onSettingChanged;
                var selectionMonitor = MonitorSourceSelectionAsync(
                    selectedSource, sourceCancellation, sourceCancellation.Token);

                try
                {
                    _logger.LogInformation("Starting earthquake source {SourceId}", selectedSource);
                    var source = _sources.GetRealtimeSource(selectedSource);
                    await foreach (var report in source.ListenAsync(sourceCancellation.Token)
                                       .WithCancellation(sourceCancellation.Token)
                                       .ConfigureAwait(false))
                    {
                        HandleReport(report, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (sourceCancellation.IsCancellationRequested)
                {
                    // Application shutdown or a requested source switch.
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Earthquake source {SourceId} failed", selectedSource);
                }
                finally
                {
                    setting.PropertyChanged -= onSettingChanged;
                    sourceCancellation.Cancel();
                    try
                    {
                        await selectionMonitor.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (sourceCancellation.IsCancellationRequested)
                    {
                        // Normal monitor shutdown.
                    }
                }

                if (!stoppingToken.IsCancellationRequested &&
                    string.Equals(selectedSource, _settings.Setting?.SourceId, StringComparison.Ordinal))
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
            }
        }
        finally
        {
            foreach (var tracker in _activeTrackers.Values)
            {
                tracker.Updates.TryComplete();
                tracker.Cancellation.Cancel();
            }

            try
            {
                await Task.WhenAll(_activeTrackers.Values.Select(value => value.Task)).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "One or more trackers had already faulted during shutdown");
            }
            foreach (var tracker in _activeTrackers.Values)
                tracker.Cancellation.Dispose();
            _activeTrackers.Clear();
        }
    }

    private async Task MonitorSourceSelectionAsync(
        string selectedSource,
        CancellationTokenSource sourceCancellation,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!string.Equals(selectedSource, _settings.Setting?.SourceId, StringComparison.Ordinal))
            {
                sourceCancellation.Cancel();
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
        }
    }

    private void HandleReport(EarthquakeReport report, CancellationToken stoppingToken)
    {
        if (report.Status == EarthquakeReportStatus.Training)
        {
            _logger.LogInformation("Ignoring training report {Provider}/{EventId}",
                report.EventKey.Provider, report.EventKey.EventId);
            return;
        }

        if (_activeTrackers.TryGetValue(report.EventKey, out var active))
        {
            if (report.Status == EarthquakeReportStatus.Cancelled)
            {
                active.Updates.TryWrite(report);
                active.Cancellation.Cancel();
                return;
            }

            if (!IsNewer(active.LastReport, report))
                return;
            active.LastReport = report;
            active.Updates.TryWrite(report);
            return;
        }

        if (report.Status == EarthquakeReportStatus.Cancelled ||
            DateTimeOffset.Now - report.OriginTime > TimeSpan.FromMinutes(5))
            return;

        var channel = Channel.CreateUnbounded<EarthquakeReport>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var tracker = _services.GetRequiredService<IEarthQuakeTracker>();
        var task = tracker.TrackAsync(report, channel.Reader, cancellation.Token);
        var activeTracker = new ActiveTracker(report, channel.Writer, cancellation, task);
        if (!_activeTrackers.TryAdd(report.EventKey, activeTracker))
        {
            channel.Writer.TryComplete();
            cancellation.Cancel();
            cancellation.Dispose();
            return;
        }

        _logger.LogInformation("Tracking earthquake {Provider}/{EventId} at {Position} with magnitude {Magnitude}",
            report.EventKey.Provider, report.EventKey.EventId, report.Hypocenter, report.Magnitude);
        _ = ObserveTrackerAsync(report.EventKey, activeTracker);
    }

    private async Task ObserveTrackerAsync(EarthquakeEventKey key, ActiveTracker tracker)
    {
        try
        {
            await tracker.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (tracker.Cancellation.IsCancellationRequested)
        {
            // Normal event cancellation.
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Tracker failed for {Provider}/{EventId}", key.Provider, key.EventId);
        }
        finally
        {
            tracker.Updates.TryComplete();
            if (_activeTrackers.TryRemove(key, out _))
                tracker.Cancellation.Dispose();
        }
    }

    private static bool IsNewer(EarthquakeReport current, EarthquakeReport candidate)
    {
        if (current.ReportNumber.HasValue && candidate.ReportNumber.HasValue)
            return candidate.ReportNumber > current.ReportNumber;
        return candidate.ReportTime > current.ReportTime;
    }

    private sealed class ActiveTracker
    {
        public ActiveTracker(
            EarthquakeReport lastReport,
            ChannelWriter<EarthquakeReport> updates,
            CancellationTokenSource cancellation,
            Task task)
        {
            LastReport = lastReport;
            Updates = updates;
            Cancellation = cancellation;
            Task = task;
        }

        public EarthquakeReport LastReport { get; set; }
        public ChannelWriter<EarthquakeReport> Updates { get; }
        public CancellationTokenSource Cancellation { get; }
        public Task Task { get; }
    }
}

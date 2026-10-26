using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Services.EarthQuakeSources;

/// <summary>Adapts a legacy request/response earthquake API to the new query and real-time contracts.</summary>
public sealed class LegacyEarthquakeSource : IEarthquakeRealtimeSource, IEarthquakeQuerySource
{
    private readonly IEarthQuakeApi _api;
    private readonly ISetting<UpdaterSetting> _settings;

    /// <summary>Initializes a legacy source adapter.</summary>
    /// <param name="sourceId">The stable source identifier.</param>
    /// <param name="api">The legacy API implementation.</param>
    /// <param name="settings">The polling settings.</param>
    public LegacyEarthquakeSource(string sourceId, IEarthQuakeApi api, ISetting<UpdaterSetting> settings)
    {
        SourceId = string.IsNullOrWhiteSpace(sourceId)
            ? throw new ArgumentException("A source identifier is required.", nameof(sourceId))
            : sourceId;
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public string SourceId { get; }

    /// <inheritdoc />
    public async IAsyncEnumerable<EarthquakeReport> ListenAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pointer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        while (!cancellationToken.IsCancellationRequested)
        {
            var values = await _api.GetEarthQuakeList(pointer, cancellationToken).ConfigureAwait(false);
            foreach (var report in values
                         .Select(value => EarthquakeReport.FromLegacyModel(SourceId, value))
                         .OrderBy(value => value.ReportTime))
            {
                pointer = Math.Max(pointer, report.ReportTime.ToUnixTimeMilliseconds() + 1);
                yield return report;
            }

            var seconds = Math.Max(3, _settings.Setting?.UpdateTimeSpanSecond ?? 5);
            await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EarthquakeReport>> GetRecentAsync(CancellationToken cancellationToken)
    {
        var values = await _api.GetEarthQuakeList(1, cancellationToken).ConfigureAwait(false);
        return values.Select(value => EarthquakeReport.FromLegacyModel(SourceId, value)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EarthquakeReport>> GetEventReportsAsync(
        EarthquakeEventKey eventKey,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(eventKey.Provider, SourceId, StringComparison.Ordinal))
            return Array.Empty<EarthquakeReport>();

        var values = await _api.GetEarthQuakeInfo(eventKey.EventId, cancellationToken).ConfigureAwait(false);
        return values.Select(value => EarthquakeReport.FromLegacyModel(SourceId, value))
            .OrderBy(value => value.ReportTime)
            .ToList();
    }
}

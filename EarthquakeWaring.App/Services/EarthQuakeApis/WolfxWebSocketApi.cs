using System;
using System.Collections.Generic;
using System.Threading;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Services.EarthQuakeSources;

namespace EarthquakeWaring.App.Services.EarthQuakeApis;

/// <summary>
/// Exposes the Wolfx all-EEW WebSocket transport as a selectable real-time earthquake source.
/// </summary>
public sealed class WolfxWebSocketApi : IEarthquakeRealtimeSource
{
    private readonly WolfxWebSocketRealtimeSource _source;

    /// <summary>Initializes the Wolfx WebSocket API source.</summary>
    /// <param name="source">The resilient Wolfx WebSocket transport.</param>
    public WolfxWebSocketApi(WolfxWebSocketRealtimeSource source) =>
        _source = source ?? throw new ArgumentNullException(nameof(source));

    /// <inheritdoc />
    public string SourceId => _source.SourceId;

    /// <inheritdoc />
    public IAsyncEnumerable<EarthquakeReport> ListenAsync(CancellationToken cancellationToken) =>
        _source.ListenAsync(cancellationToken);
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

/// <summary>Provides a continuous stream of normalized earthquake reports.</summary>
public interface IEarthquakeRealtimeSource
{
    /// <summary>Gets the stable source identifier.</summary>
    string SourceId { get; }

    /// <summary>Listens for reports until cancellation is requested.</summary>
    /// <param name="cancellationToken">Stops listening and releases source resources.</param>
    /// <returns>An asynchronous stream of reports.</returns>
    IAsyncEnumerable<EarthquakeReport> ListenAsync(CancellationToken cancellationToken);
}

/// <summary>Provides historical and event-specific earthquake queries.</summary>
public interface IEarthquakeQuerySource
{
    /// <summary>Gets the stable source identifier.</summary>
    string SourceId { get; }

    /// <summary>Gets recent earthquakes available from this source.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The available recent reports.</returns>
    Task<IReadOnlyList<EarthquakeReport>> GetRecentAsync(CancellationToken cancellationToken);

    /// <summary>Gets known reports for one provider-qualified event.</summary>
    /// <param name="eventKey">The event to query.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Known reports ordered by report time.</returns>
    Task<IReadOnlyList<EarthquakeReport>> GetEventReportsAsync(
        EarthquakeEventKey eventKey,
        CancellationToken cancellationToken);
}

/// <summary>Resolves configured earthquake source capabilities by stable identifier.</summary>
public interface IEarthquakeSourceCatalog
{
    /// <summary>Gets a real-time source.</summary>
    /// <param name="sourceId">The stable source identifier.</param>
    /// <returns>The matching real-time source.</returns>
    IEarthquakeRealtimeSource GetRealtimeSource(string sourceId);

    /// <summary>Gets a query source.</summary>
    /// <param name="sourceId">The stable source identifier.</param>
    /// <returns>The matching query source.</returns>
    IEarthquakeQuerySource GetQuerySource(string sourceId);
}

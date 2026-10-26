using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services.EarthQuakeSources;

/// <summary>Queries Wolfx HTTP endpoints for history, current snapshots, and cached event reports.</summary>
public sealed class WolfxHttpQuerySource : IEarthquakeQuerySource
{
    private static readonly IReadOnlyDictionary<string, string> ProviderEndpoints =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["sc_eew"] = "sc_eew.json",
            ["fj_eew"] = "fj_eew.json",
            ["cq_eew"] = "cq_eew.json",
            ["cenc_eew"] = "cenc_eew.json",
            ["jma_eew"] = "jma_eew.json",
            ["cenc_eqlist"] = "cenc_eqlist.json",
            ["jma_eqlist"] = "jma_eqlist.json"
        };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WolfxMessageParser _parser;
    private readonly IWolfxReportCache _cache;
    private readonly ILogger<WolfxHttpQuerySource> _logger;

    /// <summary>Initializes the Wolfx HTTP query source.</summary>
    public WolfxHttpQuerySource(
        IHttpClientFactory httpClientFactory,
        WolfxMessageParser parser,
        IWolfxReportCache cache,
        ILogger<WolfxHttpQuerySource> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string SourceId => EarthquakeSourceIds.WolfxWebSocket;

    /// <inheritdoc />
    public async Task<IReadOnlyList<EarthquakeReport>> GetRecentAsync(CancellationToken cancellationToken)
    {
        var cencTask = FetchAsync("cenc_eqlist", cancellationToken);
        var jmaTask = FetchAsync("jma_eqlist", cancellationToken);
        var results = await Task.WhenAll(cencTask, jmaTask).ConfigureAwait(false);
        return results.SelectMany(value => value)
            .OrderByDescending(report => report.OriginTime)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EarthquakeReport>> GetEventReportsAsync(
        EarthquakeEventKey eventKey,
        CancellationToken cancellationToken)
    {
        var reports = _cache.Get(eventKey).ToList();
        if (!ProviderEndpoints.ContainsKey(eventKey.Provider))
            return reports;

        var current = await FetchAsync(eventKey.Provider, cancellationToken).ConfigureAwait(false);
        foreach (var report in current.Where(report => report.EventKey == eventKey))
        {
            _cache.Add(report);
            if (!reports.Any(existing => SameReport(existing, report)))
                reports.Add(report);
        }

        return reports.OrderBy(report => report.ReportTime).ToList();
    }

    /// <summary>Gets the current snapshot from every Wolfx EEW HTTP endpoint.</summary>
    /// <param name="cancellationToken">Cancels the requests.</param>
    /// <returns>Available current reports.</returns>
    public async Task<IReadOnlyList<EarthquakeReport>> GetCurrentSnapshotsAsync(CancellationToken cancellationToken)
    {
        var providers = new[] { "sc_eew", "fj_eew", "cq_eew", "cenc_eew", "jma_eew" };
        var tasks = providers.Select(provider => FetchAsync(provider, cancellationToken)).ToArray();
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.SelectMany(value => value).ToList();
    }

    private async Task<IReadOnlyList<EarthquakeReport>> FetchAsync(
        string provider,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Wolfx");
            var json = await client.GetStringAsync(ProviderEndpoints[provider], cancellationToken).ConfigureAwait(false);
            var result = _parser.ParseHttp(json, provider);
            foreach (var report in result.Reports)
                _cache.Add(report);
            return result.Reports;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Wolfx HTTP query failed for {Provider}", provider);
            return Array.Empty<EarthquakeReport>();
        }
    }

    private static bool SameReport(EarthquakeReport left, EarthquakeReport right) =>
        left.ReportNumber.HasValue && right.ReportNumber.HasValue
            ? left.ReportNumber == right.ReportNumber
            : left.ReportTime == right.ReportTime;
}

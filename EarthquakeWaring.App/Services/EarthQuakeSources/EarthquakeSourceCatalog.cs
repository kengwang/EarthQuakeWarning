using System;
using System.Collections.Generic;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Services.EarthQuakeApis;

namespace EarthquakeWaring.App.Services.EarthQuakeSources;

/// <inheritdoc />
public sealed class EarthquakeSourceCatalog : IEarthquakeSourceCatalog
{
    private readonly IReadOnlyDictionary<string, IEarthquakeRealtimeSource> _realtimeSources;
    private readonly IReadOnlyDictionary<string, IEarthquakeQuerySource> _querySources;

    /// <summary>Initializes the catalog from the application's configured source implementations.</summary>
    public EarthquakeSourceCatalog(
        HuaniaEarthQuakeApi huania,
        SichuanEarthQuakeApi sichuan,
        WolfxSceewApi wolfxSceew,
        WolfxCencApi wolfxCenc,
        MiuiEarthQuakeApi miui,
        CencEarthQuakeApi cenc,
        CencRapidEarthQuakeApi cencRapid,
        WolfxWebSocketApi wolfxRealtime,
        WolfxHttpQuerySource wolfxQuery,
        ISetting<UpdaterSetting> settings)
    {
        ArgumentNullException.ThrowIfNull(huania);
        ArgumentNullException.ThrowIfNull(sichuan);
        ArgumentNullException.ThrowIfNull(wolfxSceew);
        ArgumentNullException.ThrowIfNull(wolfxCenc);
        ArgumentNullException.ThrowIfNull(miui);
        ArgumentNullException.ThrowIfNull(cenc);
        ArgumentNullException.ThrowIfNull(cencRapid);
        ArgumentNullException.ThrowIfNull(wolfxRealtime);
        ArgumentNullException.ThrowIfNull(wolfxQuery);
        ArgumentNullException.ThrowIfNull(settings);

        var sources = new IEarthquakeQuerySource[]
        {
            new LegacyEarthquakeSource(EarthquakeSourceIds.Huania, huania, settings),
            new LegacyEarthquakeSource(EarthquakeSourceIds.Sichuan, sichuan, settings),
            new LegacyEarthquakeSource(EarthquakeSourceIds.WolfxSceewHttp, wolfxSceew, settings),
            new LegacyEarthquakeSource(EarthquakeSourceIds.WolfxCencHttp, wolfxCenc, settings),
            new LegacyEarthquakeSource(EarthquakeSourceIds.Miui, miui, settings),
            new LegacyEarthquakeSource(EarthquakeSourceIds.CencHttp, cenc, settings),
            new LegacyEarthquakeSource(EarthquakeSourceIds.CencRapidHttp, cencRapid, settings),
            wolfxQuery
        };

        var realtime = new IEarthquakeRealtimeSource[]
        {
            (IEarthquakeRealtimeSource)sources[0],
            (IEarthquakeRealtimeSource)sources[1],
            (IEarthquakeRealtimeSource)sources[2],
            (IEarthquakeRealtimeSource)sources[3],
            (IEarthquakeRealtimeSource)sources[4],
            (IEarthquakeRealtimeSource)sources[5],
            (IEarthquakeRealtimeSource)sources[6],
            wolfxRealtime
        };

        _querySources = ToDictionary(sources);
        _realtimeSources = ToDictionary(realtime);
    }

    /// <inheritdoc />
    public IEarthquakeRealtimeSource GetRealtimeSource(string sourceId) =>
        _realtimeSources.TryGetValue(sourceId, out var source)
            ? source
            : throw new KeyNotFoundException($"Unknown earthquake source '{sourceId}'.");

    /// <inheritdoc />
    public IEarthquakeQuerySource GetQuerySource(string sourceId) =>
        _querySources.TryGetValue(sourceId, out var source)
            ? source
            : throw new KeyNotFoundException($"Unknown earthquake source '{sourceId}'.");

    private static Dictionary<string, TSource> ToDictionary<TSource>(IEnumerable<TSource> sources)
        where TSource : class => sources switch
        {
            IEnumerable<IEarthquakeRealtimeSource> realtime => BuildRealtime<TSource>(realtime),
            IEnumerable<IEarthquakeQuerySource> query => BuildQuery<TSource>(query),
            _ => throw new InvalidOperationException("Unsupported earthquake source collection.")
        };

    private static Dictionary<string, TSource> BuildRealtime<TSource>(IEnumerable<IEarthquakeRealtimeSource> sources)
        where TSource : class
    {
        var result = new Dictionary<string, TSource>(StringComparer.Ordinal);
        foreach (var source in sources)
            result.Add(source.SourceId, (TSource)source);
        return result;
    }

    private static Dictionary<string, TSource> BuildQuery<TSource>(IEnumerable<IEarthquakeQuerySource> sources)
        where TSource : class
    {
        var result = new Dictionary<string, TSource>(StringComparer.Ordinal);
        foreach (var source in sources)
            result.Add(source.SourceId, (TSource)source);
        return result;
    }
}

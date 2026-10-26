using System.Collections.Generic;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels;

/// <summary>Stable identifiers for selectable earthquake data sources.</summary>
public static class EarthquakeSourceIds
{
    /// <summary>Chengdu Hi-tech Institute of Disaster Reduction.</summary>
    public const string Huania = "huania";
    /// <summary>Sichuan Earthquake Administration HTTP API.</summary>
    public const string Sichuan = "sichuan";
    /// <summary>Wolfx Sichuan HTTP API.</summary>
    public const string WolfxSceewHttp = "wolfx-sceew-http";
    /// <summary>Wolfx CENC HTTP API.</summary>
    public const string WolfxCencHttp = "wolfx-cenc-http";
    /// <summary>MIUI earthquake warning API.</summary>
    public const string Miui = "miui";
    /// <summary>China Earthquake Networks Center HTTP API.</summary>
    public const string CencHttp = "cenc-http";
    /// <summary>China Earthquake Networks Center rapid report HTTP API.</summary>
    public const string CencRapidHttp = "cenc-rapid-http";
    /// <summary>Wolfx all-EEW WebSocket with Wolfx HTTP queries.</summary>
    public const string WolfxWebSocket = "wolfx-websocket";

    /// <summary>Gets the legacy index-to-identifier mapping.</summary>
    public static IReadOnlyList<string> LegacyOrder { get; } = new[]
    {
        Huania, Sichuan, WolfxSceewHttp, WolfxCencHttp, Miui, CencHttp, CencRapidHttp
    };

    /// <summary>Maps a legacy selected index to a stable identifier.</summary>
    /// <param name="apiType">The legacy selected index.</param>
    /// <returns>The corresponding stable identifier, or the default source.</returns>
    public static string FromLegacyIndex(int apiType) =>
        apiType >= 0 && apiType < LegacyOrder.Count ? LegacyOrder[apiType] : Huania;
}

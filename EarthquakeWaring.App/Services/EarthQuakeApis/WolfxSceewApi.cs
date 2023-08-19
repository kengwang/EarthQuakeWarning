using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Services.EarthQuakeApis;

public class WolfxSceewApi : IEarthQuakeApi
{
    public string ApiUrl = "https://api.wolfx.jp/sc_eew.json";
    private readonly IHttpRequester _httpRequester;
    private readonly IJsonConvertService _jsonConvertService;

    public WolfxSceewApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService)
    {
        _httpRequester = httpRequester;
        _jsonConvertService = jsonConvertService;
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeList(long startTimePointer,
                                                                  CancellationToken cancellationToken)
    {
        var result = await _httpRequester.GetString(ApiUrl, null, cancellationToken);
        var ret = _jsonConvertService.ConvertTo<WolfxSceewResponse>(result);
        if (ret is null)
            return new List<EarthQuakeInfoBase>();
        if (DateTimeOffset.FromFileTime(ret.UpdateTime.ToFileTime()).ToUnixTimeMilliseconds() < startTimePointer)
            return new List<EarthQuakeInfoBase>();
        return new List<EarthQuakeInfoBase> { ret.MapToEarthQuakeInfo() };
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId,
                                                                  CancellationToken cancellationToken)
    {
        var result = await _httpRequester.GetString(ApiUrl, null, cancellationToken);
        var ret = _jsonConvertService.ConvertTo<WolfxSceewResponse>(result);
        if (ret?.Id.ToString() != earthQuakeId)
            return new List<EarthQuakeInfoBase>();
        return new List<EarthQuakeInfoBase> { ret.MapToEarthQuakeInfo() };

    }
}

public class WolfxSceewResponse
{
    [JsonPropertyName("ID")] public long Id { get; set; }
    [JsonPropertyName("HypoCenter")] public string? PlaceName { get; set; }
    [JsonPropertyName("Latitude")] public double Latitude { get; set; }
    [JsonPropertyName("Longitude")] public double Longitude { get; set; }
    [JsonPropertyName("Magunitude")] public double Magunitude { get; set; }
    [JsonPropertyName("MaxIntensity")] public double MaxIntensity { get; set; }
    [JsonPropertyName("OriginTime")] public DateTime StartTime { get; set; }
    [JsonPropertyName("ReportTime")] public DateTime UpdateTime { get; set; }
}

public static class WolfxSceewResponseToEarthQuakeInfoBaseMapper
{
    public static EarthQuakeInfoBase MapToEarthQuakeInfo(this WolfxSceewResponse res)
    {
        return new EarthQuakeInfoBase
               {
                   Id = res.Id.ToString(),
                   StartAt = res.StartTime,
                   UpdateAt = res.UpdateTime,
                   Latitude = res.Latitude,
                   Longitude = res.Longitude,
                   Magnitude = res.Magunitude,
                   Depth = 0,
                   PlaceName = res.PlaceName
               };
    }
}
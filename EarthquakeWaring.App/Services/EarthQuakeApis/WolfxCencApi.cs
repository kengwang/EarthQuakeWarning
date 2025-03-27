using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Services.EarthQuakeApis;

public class WolfxCencApi : IEarthQuakeApi
{

    public string ApiUrl = "https://api.wolfx.jp/cenc_eqlist.json";

    private readonly IJsonConvertService _jsonConvertService;
    private readonly IHttpRequester _httpRequester;
    
    public WolfxCencApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService)
    {
        _httpRequester = httpRequester;
        _jsonConvertService = jsonConvertService;
    }
    
    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeList(long startTimePointer, CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(5000);
        var result = await _httpRequester.GetString(ApiUrl, cancellationToken: cts.Token);
        result = Regex.Replace(result, ",\\s?\"md5\"\\s?:\\s?\".*\"", string.Empty);
        var ret = _jsonConvertService.ConvertTo<Dictionary<string,WolfxCencItem>>(result);
        if (ret is null)
            return new List<EarthQuakeInfoBase>();
        return ret.Values.Select(t=>t.MapToEarthQuakeInfo()).Where(t=>DateTimeOffset.FromFileTime(t.UpdateAt.ToFileTime()).ToUnixTimeMilliseconds() >= startTimePointer).ToList();
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId, CancellationToken cancellationToken)
    {
        var result = await _httpRequester.GetString(ApiUrl, cancellationToken: cancellationToken);
        result = Regex.Replace(result, ", \"md5\": \".*\"", string.Empty);
        var ret = _jsonConvertService.ConvertTo<Dictionary<string,WolfxCencItem>>(result);
        if (ret is null)
            return new List<EarthQuakeInfoBase>();
        return ret.Values.Select(t=>t.MapToEarthQuakeInfo()).Where(t=>t.Id == earthQuakeId).ToList();

    }
}

public class WolfxCencItem
{
    [JsonPropertyName("time")] public DateTime Time { get; set; }
    [JsonPropertyName("location")] public string? PlaceName { get; set; }
    [JsonPropertyName("magnitude")] public double Magnitude { get; set; }
    [JsonPropertyName("depth")] public int Depth { get; set; }
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
}

public static class WolfxCencItemToEarthQuakeInfoMapper
{

    public static EarthQuakeInfoBase MapToEarthQuakeInfo(this WolfxCencItem item)
    {
        return new EarthQuakeInfoBase
               {
                   Id = item.Time.ToFileTime().ToString(),
                   StartAt = item.Time,
                   UpdateAt = item.Time,
                   Latitude = item.Latitude,
                   Longitude = item.Longitude,
                   Magnitude = item.Magnitude,
                   Depth = item.Depth,
                   PlaceName = item.PlaceName
               };
    }
}
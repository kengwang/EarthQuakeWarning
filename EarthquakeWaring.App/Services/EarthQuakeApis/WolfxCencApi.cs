using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services.EarthQuakeApis;

public class WolfxCencApi : IEarthQuakeApi
{
    public string ApiUrl = "https://api.wolfx.jp/cenc_eqlist.json";

    private readonly IJsonConvertService _jsonConvertService;
    private readonly IHttpRequester _httpRequester;
    private readonly ILogger<WolfxCencApi> _logger;

    public WolfxCencApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService,
        ILogger<WolfxCencApi> logger)
    {
        _httpRequester = httpRequester;
        _jsonConvertService = jsonConvertService;
        _logger = logger;
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeList(long startTimePointer,
        CancellationToken cancellationToken)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(5000);
            var result = await _httpRequester.GetString(ApiUrl, cancellationToken: cts.Token);
            result = Regex.Replace(result, ",\\s?\"md5\"\\s?:\\s?\".*\"", string.Empty);
            var ret = _jsonConvertService.ConvertTo<Dictionary<string, WolfxCencItem>>(result);
            if (ret is null)
                return new List<EarthQuakeInfoBase>();
            return ret.Values.Select(t => t.MapToEarthQuakeInfo()).Where(t =>
                    DateTimeOffset.FromFileTime(t.UpdateAt.ToFileTime()).ToUnixTimeMilliseconds() >= startTimePointer)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting earthquake list from WolfxCencApi");
        }

        return new List<EarthQuakeInfoBase>();
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _httpRequester.GetString(ApiUrl, cancellationToken: cancellationToken);
            result = Regex.Replace(result, ", \"md5\": \".*\"", string.Empty);
            var ret = _jsonConvertService.ConvertTo<Dictionary<string, WolfxCencItem>>(result);
            if (ret is null)
                return new List<EarthQuakeInfoBase>();
            return ret.Values.Select(t => t.MapToEarthQuakeInfo()).Where(t => t.Id == earthQuakeId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting earthquake info from WolfxCencApi");
        }

        return new List<EarthQuakeInfoBase>();
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
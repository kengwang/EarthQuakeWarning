using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services.EarthQuakeApis;

public class SichuanEarthQuakeApi : IEarthQuakeApi
{
    public string ApiUrl =
        Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cDovLzExOC4xMTMuMTA1LjI5OjgwMDIvYXBpLw=="));

    private readonly IHttpRequester _httpRequester;
    private readonly IJsonConvertService _jsonConvertService;
    private readonly ILogger<SichuanEarthQuakeApi> _logger;

    public SichuanEarthQuakeApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService,
                                ILogger<SichuanEarthQuakeApi> logger)
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
            var result = await _httpRequester.GetString(ApiUrl + "earlywarning/jsonPageList",
                                                        new Dictionary<string, string>()
                                                        {
                                                            { "orderType", "1" },
                                                            { "pageNo", "1" },
                                                            { "pageSize", "20" }
                                                        }, cancellationToken);
            var ret = _jsonConvertService.ConvertTo<SichuanEarthQuakeApiListResponse>(result);
            if (ret?.Code != 0)
            {
                throw new Exception($"Return with code {ret?.Message} ({ret?.Code})");
            }

            if (ret.Data is null)
                return new List<EarthQuakeInfoBase>();
            return ret.Data.Select(t => t.MapToEarthQuakeInfo()).Where(t=>DateTimeOffset.FromFileTime(t.UpdateAt.ToFileTime()).ToUnixTimeMilliseconds() >= startTimePointer).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling Sichuan EarthQuake Api");
            throw;
        }
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId,
                                                                  CancellationToken cancellationToken)
    {
        try
        {
            var result = await _httpRequester.GetString(ApiUrl + "earlywarning/getEarlywarningInfo",
                                                        new Dictionary<string, string>()
                                                        {
                                                            { "id", earthQuakeId }
                                                        }, cancellationToken);
            var ret = _jsonConvertService.ConvertTo<SichuanEarthQuakeApiInfoResponse>(result);
            if (ret?.Code != 0)
            {
                throw new Exception($"Return with code {ret?.Message} ({ret?.Code})");
            }
            return ret.Data?.EarthQuakeWarnings?.Select(t => t.MapToEarthQuakeInfo()).ToList() ?? new List<EarthQuakeInfoBase>();

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling Sichuan EarthQuake Api");
            throw;
        }
    }
}

public class SichuanEarthQuakeApiListResponse
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("msg")] public string? Message { get; set; }
    [JsonPropertyName("data")] public SichuanEarthQuakeApiListResponseDataItem[]? Data { get; set; }
}

public class SichuanEarthQuakeApiInfoResponse
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("msg")] public string? Message { get; set; }
    [JsonPropertyName("data")] public SichuanEarthQuakeApiInfoResponseData? Data { get; set; }

    public class SichuanEarthQuakeApiInfoResponseData
    {
        [JsonPropertyName("earlyWarning")]
        public SichuanEarthQuakeApiListResponseDataItem[]? EarthQuakeWarnings { get; set; }
    }
}

public class SichuanEarthQuakeApiListResponseDataItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("shockTime")] public DateTime ShockTime { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("placeName")] public string? PlaceName { get; set; }
    [JsonPropertyName("magnitude")] public double Magnitude { get; set; }
    [JsonPropertyName("createTimeMs")] public DateTime CreateTime { get; set; }
    [JsonPropertyName("epiIntensity")] public double EpicenterIntensity { get; set; }
}

public class SichuanEarthQuakeApiListResponseDataPrecalculatedItem : SichuanEarthQuakeApiListResponseDataItem
{
    [JsonPropertyName("intensity")] public string? Intensity { get; set; }
    [JsonPropertyName("eqFeel")] public string? EarthQuakeFeel { get; set; }
    [JsonPropertyName("propertyName")] public string? Distance { get; set; }
    [JsonPropertyName("countdownTime")] public string? CountdownTime { get; set; }
}

public static class SichuanEarthQuakeApiDataToEarthQuakeInfoMapper
{
    public static EarthQuakeInfoBase MapToEarthQuakeInfo(this SichuanEarthQuakeApiListResponseDataItem item)
    {
        return new EarthQuakeInfoBase
               {
                   Id = item.Id.ToString(),
                   StartAt = item.ShockTime,
                   UpdateAt = item.CreateTime,
                   Latitude = item.Latitude,
                   Longitude = item.Longitude,
                   Magnitude = item.Magnitude,
                   Depth = 0,
                   PlaceName = item.PlaceName
               };
    }
}
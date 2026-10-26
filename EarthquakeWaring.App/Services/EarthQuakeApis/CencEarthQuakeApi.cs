using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services.EarthQuakeApis;

/// <summary>Queries the China Earthquake Networks Center CDN API.</summary>
public class CencEarthQuakeApi : IEarthQuakeApi
{
    private const string ApiBaseUrl = "https://dzyj.cenc.ac.cn/api/earthquake/event/cdn/v1/";
    private const string AppId = "dkcxbftqof0h";

    private readonly IHttpRequester _httpRequester;
    private readonly IJsonConvertService _jsonConvertService;
    private readonly ILogger<CencEarthQuakeApi> _logger;
    private readonly int _alarmType;

    public CencEarthQuakeApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService,
        ILogger<CencEarthQuakeApi> logger)
        : this(httpRequester, jsonConvertService, logger, 1)
    {
    }

    protected CencEarthQuakeApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService,
        ILogger<CencEarthQuakeApi> logger, int alarmType)
    {
        _httpRequester = httpRequester;
        _jsonConvertService = jsonConvertService;
        _logger = logger;
        _alarmType = alarmType;
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeList(long startTimePointer,
        CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(5));
            var response = await _httpRequester.GetString(ApiBaseUrl + "list", new Dictionary<string, string>
            {
                ["app_id"] = AppId,
                ["alarm_type"] = _alarmType.ToString(),
                ["page_no"] = "1",
                ["page_size"] = "10",
                // The CDN can retain old responses for a fixed URL.
                ["v"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
            }, timeoutSource.Token).ConfigureAwait(false);
            var result = _jsonConvertService.ConvertTo<CencListResponse>(response);
            if (result?.Code != 0)
                throw new InvalidOperationException($"CENC API returned {result?.Code}: {result?.Message}");

            return result.Data?.SpotInfos?
                       .Where(item => item.CreatedAt * 1000 >= startTimePointer)
                       .Select(item => item.MapToEarthQuakeInfo())
                       .ToList()
                   ?? new List<EarthQuakeInfoBase>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while getting earthquake list from CencEarthQuakeApi");
            return new List<EarthQuakeInfoBase>();
        }
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpRequester.GetString(ApiBaseUrl + "get",
                new Dictionary<string, string>
                {
                    ["id"] = earthQuakeId,
                    ["v"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
                }, cancellationToken).ConfigureAwait(false);
            var result = _jsonConvertService.ConvertTo<CencDetailResponse>(response);
            if (result?.Code != 0)
                throw new InvalidOperationException($"CENC API returned {result?.Code}: {result?.Message}");

            return result.Data?.DetailInfo is { } detail && detail.Id == earthQuakeId
                ? new List<EarthQuakeInfoBase> { detail.MapToEarthQuakeInfo() }
                : new List<EarthQuakeInfoBase>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while getting earthquake info from CencEarthQuakeApi");
            return new List<EarthQuakeInfoBase>();
        }
    }
}

/// <summary>Queries CENC rapid earthquake reports.</summary>
public sealed class CencRapidEarthQuakeApi : CencEarthQuakeApi
{
    public CencRapidEarthQuakeApi(IHttpRequester httpRequester, IJsonConvertService jsonConvertService,
        ILogger<CencEarthQuakeApi> logger)
        : base(httpRequester, jsonConvertService, logger, 2)
    {
    }
}

public sealed class CencListResponse
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("msg")] public string? Message { get; set; }
    [JsonPropertyName("data")] public CencListData? Data { get; set; }
}

public sealed class CencListData
{
    [JsonPropertyName("spot_infos")] public List<CencEarthQuakeItem>? SpotInfos { get; set; }
}

public sealed class CencDetailResponse
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("msg")] public string? Message { get; set; }
    [JsonPropertyName("data")] public CencDetailData? Data { get; set; }
}

public sealed class CencDetailData
{
    [JsonPropertyName("detail_info")] public CencEarthQuakeItem? DetailInfo { get; set; }
}

public sealed class CencEarthQuakeItem
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("location")] public string? Location { get; set; }
    [JsonPropertyName("level")] public double Level { get; set; }
    [JsonPropertyName("depth")] public double Depth { get; set; }
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
    [JsonPropertyName("created_at")] public long CreatedAt { get; set; }
}

public static class CencEarthQuakeItemMapper
{
    public static EarthQuakeInfoBase MapToEarthQuakeInfo(this CencEarthQuakeItem item)
    {
        var time = DateTimeOffset.FromUnixTimeSeconds(item.CreatedAt).LocalDateTime;
        return new EarthQuakeInfoBase
        {
            Id = item.Id,
            StartAt = time,
            UpdateAt = time,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            Magnitude = item.Level,
            Depth = item.Depth,
            PlaceName = item.Location
        };
    }
}

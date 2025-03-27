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

public class HuaniaEarthQuakeApi : IEarthQuakeApi
{
    private readonly IHttpRequester _httpRequester;
    private readonly IJsonConvertService _jsonConvert;
    private readonly ILogger<HuaniaEarthQuakeApi> _logger;

    private string HuaniaApi =
        Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9tb2JpbGUtbmV3LmNoaW5hZWV3LmNuL3YxLw=="));

    public HuaniaEarthQuakeApi(IHttpRequester httpRequester, IJsonConvertService jsonConvert,
                               ILogger<HuaniaEarthQuakeApi> logger)
    {
        _httpRequester = httpRequester;
        _jsonConvert = jsonConvert;
        _logger = logger;
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeList(long startPointer,
                                                                  CancellationToken cancellationToken)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(5000);
            var response = _jsonConvert.ConvertTo<HuaniaWarningsResponse>(
                await _httpRequester.GetString(HuaniaApi + "earlywarnings?updates=3&start_at=" + startPointer, null,
                    cts.Token).ConfigureAwait(false));
            if (response?.Code != 0)
                throw new Exception(response?.Message);
            return response.Data.Select(t => t.MapToEarthQuakeInfo()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling Huania Api");
        }

        return new();
    }

    public async Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId,
                                                                  CancellationToken cancellationToken)
    {
        try
        {
            var response = _jsonConvert.ConvertTo<HuaniaEarthQuakeInfoResponse>(
                await _httpRequester.GetString(HuaniaApi + "earlywarnings/" + earthQuakeId, null,
                                               cancellationToken));
            if (response?.Code != 0)
                throw new Exception(response?.Message);
            return response.Data.Select(t => t.MapToEarthQuakeInfo()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling Huania Api");
        }

        return new();
    }
}

public class HuaniaEarthQuakeDto
{
    [JsonPropertyName("eventId")] public long EventId { get; set; }

    [JsonPropertyName("updates")] public long Updates { get; set; }

    [JsonPropertyName("latitude")] public double Latitude { get; set; }

    [JsonPropertyName("longitude")] public double Longitude { get; set; }

    [JsonPropertyName("depth")] public double Depth { get; set; }

    [JsonPropertyName("epicenter")] public string Epicenter { get; set; }

    [JsonPropertyName("startAt")] public DateTime StartAt { get; set; }

    [JsonPropertyName("updateAt")] public DateTime UpdateAt { get; set; }

    [JsonPropertyName("magnitude")] public double Magnitude { get; set; }

    [JsonPropertyName("insideNet")] public long InsideNet { get; set; }

    [JsonPropertyName("sations")] public long Sations { get; set; }
}

public class HuaniaEarthQuakeInfoResponse
{
    [JsonPropertyName("code")] public long Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("data")] public List<HuaniaEarthQuakeDto> Data { get; set; }
}

public class HuaniaWarningsResponse
{
    [JsonPropertyName("code")] public long Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("data")] public List<HuaniaEarthQuakeDto> Data { get; set; }
}

public static class HuaniaEarthQuakeToEarthQuakeInfoMapper
{
    public static EarthQuakeInfoBase MapToEarthQuakeInfo(this HuaniaEarthQuakeDto earthQuake)
    {
        return new EarthQuakeInfoBase
               {
                   Id = earthQuake.EventId.ToString(),
                   StartAt = earthQuake.StartAt,
                   Latitude = earthQuake.Latitude,
                   Longitude = earthQuake.Longitude,
                   Magnitude = earthQuake.Magnitude,
                   Depth = earthQuake.Depth,
                   PlaceName = earthQuake.Epicenter
               };
    }
}
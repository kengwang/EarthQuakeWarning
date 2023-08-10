using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EarthquakeWaring.App.Infrastructure.Models.ApiModels;

public class HuaniaEarthQuakeInfoResponse
{
    [JsonPropertyName("code")] public long Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("data")] public List<EarthQuakeUpdate> Data { get; set; }
}
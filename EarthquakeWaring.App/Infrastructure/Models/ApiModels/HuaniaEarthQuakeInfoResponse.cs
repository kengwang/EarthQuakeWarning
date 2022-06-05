using System.Collections.Generic;
using System.Text.Json.Serialization;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.Models.ApiModels;

public class HuaniaEarthQuakeInfoResponse
{
    [JsonPropertyName("code")] public long Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("data")] public List<EarthQuakeUpdate> Data { get; set; }
}
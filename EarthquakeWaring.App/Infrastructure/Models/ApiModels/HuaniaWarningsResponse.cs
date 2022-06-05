using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EarthquakeWaring.App.Infrastructure.Models.ApiModels;

public class HuaniaWarningsResponse
{
    [JsonPropertyName("code")] public long Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("data")] public List<HuaniaEarthQuake> Data { get; set; }
}
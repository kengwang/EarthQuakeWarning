using System;
using System.Text.Json.Serialization;
using EarthquakeWaring.App.Infrastructure.Models.BaseModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.Models.ApiModels;

public class HuaniaEarthQuakeDto
{
    [JsonPropertyName("eventId")]
    public long EventId { get; set; }

    [JsonPropertyName("updates")]
    public long Updates { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("depth")]
    public double Depth { get; set; }

    [JsonPropertyName("epicenter")]
    public string Epicenter { get; set; }

    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("updateAt")]
    public DateTime UpdateAt { get; set; }

    [JsonPropertyName("magnitude")]
    public double Magnitude { get; set; }

    [JsonPropertyName("insideNet")]
    public long InsideNet { get; set; }

    [JsonPropertyName("sations")]
    public long Sations { get; set; }
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
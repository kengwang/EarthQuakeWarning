using System;
using System.Text.Json.Serialization;

namespace EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

public class EarthQuakeUpdate
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
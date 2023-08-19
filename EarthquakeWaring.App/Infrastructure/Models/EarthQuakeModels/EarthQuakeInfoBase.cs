using System;

namespace EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

public class EarthQuakeInfoBase
{
    public string Id { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Magnitude { get; set; }
    public double Depth { get; set; }
    public string? PlaceName { get; set; }
}
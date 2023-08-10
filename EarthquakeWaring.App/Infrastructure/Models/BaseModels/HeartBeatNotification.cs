using System;

namespace EarthquakeWaring.App.Infrastructure.Models.BaseModels;

public class HeartBeatNotification : INotification
{
    public HeartBeatNotification(DateTime time)
    {
        Time = time;
    }

    public DateTime Time { get; }
}
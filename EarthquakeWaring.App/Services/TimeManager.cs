using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;

namespace EarthquakeWaring.App.Services
{
    public class TimeManager : ITimeHandler
    {
        public TimeSpan Offset { get; set; } = TimeSpan.Zero;
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public Timer Timer { get; }

        public TimeManager(ISetting<TimeSetting> setting, ILogger<TimeManager> logger)
        {
            var interval = TimeSpan.FromMinutes(setting.Setting?.TimeCheckInterval ?? 30d);
            Timer = new Timer(interval.TotalMilliseconds);
            Timer.AutoReset = true;
            Timer.Start();
        }

    }
}

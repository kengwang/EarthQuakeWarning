using System;
using System.Timers;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction
{
    public interface ITimeHandler
    {
        public TimeSpan Offset { get; set; }
        public DateTime LastUpdated { get; set; }
        public Timer Timer { get; }
    }
}

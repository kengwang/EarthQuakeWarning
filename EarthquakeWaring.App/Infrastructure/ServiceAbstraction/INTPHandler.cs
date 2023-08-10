using System;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction
{
    public interface INTPHandler
    {
        public TimeSpan Offset { get; }
        public string NTPServer { get; }
        public DateTime LastUpdated { get; }
        public Task<bool> GetNTPServerTime(CancellationToken ctk = default);
        public bool TrySetSystemTime(DateTime dateTime);
    }
}

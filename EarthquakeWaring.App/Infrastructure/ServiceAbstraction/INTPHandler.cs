using System;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction
{
    public interface INTPHandler
    {
        public string NTPServer { get; }
        public Task<bool> GetNTPServerTime(CancellationToken ctk = default);
        public bool TrySetSystemTime(DateTime dateTime);
    }
}

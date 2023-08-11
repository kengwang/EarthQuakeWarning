using NmeaParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction
{
    public interface IGNSSHandler
    {
        public Task<bool> GetCurrentInfoAsync(CancellationToken token = default);
        public bool TrySetSystemTime(DateTime dateTime);
        public NmeaDevice? NMEADevice { get; set; }
    }
}

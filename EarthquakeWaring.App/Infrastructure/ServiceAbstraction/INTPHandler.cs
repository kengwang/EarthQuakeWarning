using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction
{
    public interface INTPHandler
    {
        public TimeSpan Offset { get;}
        public string NTPServer { get;}
        public Task<bool> GetNTPServerTime();
        public bool TrySetSystemTime(DateTime dateTime);
    }
}

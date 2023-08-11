using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction
{
    public interface ITimeHandler
    {
        public TimeSpan Offset { get; set; }
        public Timer Timer { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeTracker
{
    public TimeSpan SimulateTimeSpan { get; set; }
    public List<EarthQuakeUpdate>? SimulateUpdates { get; set; }
    
    Task StartTrack(HuaniaEarthQuake huaniaEarthQuake,CancellationTokenSource cancellationToken);
}
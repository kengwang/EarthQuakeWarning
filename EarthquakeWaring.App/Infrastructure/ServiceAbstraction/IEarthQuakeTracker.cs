using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeTracker
{
    public TimeSpan SimulateTimeSpan { get; set; }
    public List<EarthQuakeUpdate>? SimulateUpdates { get; set; }

    Task StartTrack(HuaniaEarthQuake huaniaEarthQuake, CancellationTokenSource cancellationToken);
}
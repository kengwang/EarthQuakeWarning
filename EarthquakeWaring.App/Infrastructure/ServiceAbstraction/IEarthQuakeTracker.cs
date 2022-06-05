using System;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.ApiModels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeTracker
{
    public TimeSpan SimulateTimeSpan { get; set; }
    
    Task StartTrack(HuaniaEarthQuake huaniaEarthQuake,CancellationTokenSource cancellationToken);
}
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeTracker
{
    public TimeSpan SimulateTimeSpan { get; set; }
    public IReadOnlyList<EarthquakeReport>? SimulateReports { get; set; }

    Task TrackAsync(
        EarthquakeReport initialReport,
        ChannelReader<EarthquakeReport> updates,
        CancellationToken cancellationToken);
}

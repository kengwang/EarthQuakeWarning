﻿using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.BaseModels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeTracker
{
    public TimeSpan SimulateTimeSpan { get; set; }
    public List<EarthQuakeInfoBase>? SimulateUpdates { get; set; }

    Task StartTrack(EarthQuakeInfoBase earthQuakeInfo, CancellationTokenSource cancellationToken);
}
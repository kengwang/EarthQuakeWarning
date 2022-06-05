using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.BaseModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.DependencyInjection;

namespace EarthquakeWaring.App.Services;

public class EarthQuakeInfoUpdater : INotificationHandler<HeartBeatNotification>, IEarthQuakeInfoUpdater
{
    private readonly IEarthQuakeApi _earthQuakeApi;
    private readonly IServiceProvider _serviceProvider;

#if DEBUG
    private long _lastEarthQuakeId = 1654275714090;
#else
    private long _lastEarthQuakeId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
#endif
    private readonly Dictionary<IEarthQuakeTracker, CancellationTokenSource> _trackers = new();

    public EarthQuakeInfoUpdater(IEarthQuakeApi earthQuakeApi, IServiceProvider serviceProvider)
    {
        _earthQuakeApi = earthQuakeApi;
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(HeartBeatNotification notification, CancellationToken cancellationToken)
    {
        var quakeList = await _earthQuakeApi.GetEarthQuakeList(_lastEarthQuakeId, cancellationToken)
            .ConfigureAwait(false);
        if (quakeList.Count <= 0) return;
        foreach (var earthQuake in quakeList)
        {
            var tracker = _serviceProvider.GetService<IEarthQuakeTracker>();
            var trackCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _trackers[tracker!] = trackCancellationTokenSource;
            tracker?.StartTrack(earthQuake, trackCancellationTokenSource);
        }

        _lastEarthQuakeId = DateTimeOffset.FromFileTime(quakeList[0].StartAt.ToFileTime()).ToUnixTimeMilliseconds() + 1;
    }
}
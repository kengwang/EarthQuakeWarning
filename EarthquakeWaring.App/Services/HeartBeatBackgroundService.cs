using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.BaseModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Hosting;

namespace EarthquakeWaring.App.Services;

public class HeartBeatBackgroundService : BackgroundService
{
    private readonly INotificationPublisher _publisher;

    public HeartBeatBackgroundService(INotificationPublisher publisher)
    {
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _publisher.Publish(new HeartBeatNotification(DateTime.Now), stoppingToken).ConfigureAwait(false);
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
        }
    }
}
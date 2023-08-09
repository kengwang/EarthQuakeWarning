using EarthquakeWaring.App.Infrastructure.Models.BaseModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Services;

public class HeartBeatBackgroundService : BackgroundService
{
    private readonly INotificationPublisher _publisher;
    private readonly INTPHandler _ntpClient;

    public HeartBeatBackgroundService(INotificationPublisher publisher, INTPHandler ntpClient)
    {
        _publisher = publisher;
        _ntpClient = ntpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _publisher.Publish(new HeartBeatNotification(DateTime.Now + _ntpClient!.Offset), stoppingToken).ConfigureAwait(false);
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
        }
    }
}
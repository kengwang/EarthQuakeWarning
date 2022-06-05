using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly IServiceProvider _service;

    public NotificationPublisher(IServiceProvider service)
    {
        _service = service;
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
    {
        var services = (IEnumerable<INotificationHandler<TNotification>>)_service.GetService(typeof(IEnumerable<INotificationHandler<TNotification>>))!;
        foreach (var notificationHandler in services)
        {
            await notificationHandler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
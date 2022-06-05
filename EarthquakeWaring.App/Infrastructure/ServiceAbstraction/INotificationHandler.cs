using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface INotificationHandler<in TNotification>
{
    public Task Handle(TNotification notification, CancellationToken cancellationToken);
}
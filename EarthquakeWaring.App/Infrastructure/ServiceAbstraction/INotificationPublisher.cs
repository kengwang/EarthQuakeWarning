using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface INotificationPublisher
{
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken);
}
using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeApi
{
    Task<List<HuaniaEarthQuake>> GetEarthQuakeList(long startPointer, CancellationToken cancellationToken);
    Task<List<EarthQuakeUpdate>> GetEarthQuakeInfo(long earthQuakeId, CancellationToken cancellationToken);
}
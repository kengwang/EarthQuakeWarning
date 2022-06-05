using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeApi
{
    Task<List<HuaniaEarthQuake>> GetEarthQuakeList(long startPointer, CancellationToken cancellationToken);
    Task<List<EarthQuakeUpdate>> GetEarthQuakeInfo(long earthQuakeId, CancellationToken cancellationToken);
}
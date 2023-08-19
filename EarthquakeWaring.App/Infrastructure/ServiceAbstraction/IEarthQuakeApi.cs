using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.BaseModels;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IEarthQuakeApi
{
    Task<List<EarthQuakeInfoBase>> GetEarthQuakeList(long startTimePointer, CancellationToken cancellationToken);
    Task<List<EarthQuakeInfoBase>> GetEarthQuakeInfo(string earthQuakeId, CancellationToken cancellationToken);
}
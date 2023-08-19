using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IHttpRequester
{
    Task<string> GetString(string url, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task<string> PostString(string url, HttpContent? data = null, CancellationToken cancellationToken = default);
}
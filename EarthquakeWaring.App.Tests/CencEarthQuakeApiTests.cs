using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Services;
using EarthquakeWaring.App.Services.EarthQuakeApis;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarthquakeWaring.App.Tests;

[TestClass]
public sealed class CencEarthQuakeApiTests
{
    [TestMethod]
    public async Task GetEarthQuakeList_MapsAndFiltersOfficialReports()
    {
        var requester = new StubHttpRequester
        {
            Response = "{\"code\":0,\"data\":{\"spot_infos\":[" +
                       "{\"id\":\"new\",\"location\":\"四川\",\"level\":\"4.2\",\"depth\":10," +
                       "\"latitude\":30,\"longitude\":104,\"created_at\":200}," +
                       "{\"id\":\"old\",\"location\":\"云南\",\"level\":\"3.1\",\"created_at\":100}]}}"
        };
        var api = new CencEarthQuakeApi(requester, new JsonConvertService(),
            NullLogger<CencEarthQuakeApi>.Instance);

        var values = await api.GetEarthQuakeList(150_000, CancellationToken.None);

        values.Should().ContainSingle();
        var value = values[0];
        value.Id.Should().Be("new");
        value.Magnitude.Should().Be(4.2);
        value.Depth.Should().Be(10);
        value.PlaceName.Should().Be("四川");
        requester.LastUrl.Should().Contain("alarm_type=1");
        requester.LastUrl.Should().Contain("&v=");
    }

    [TestMethod]
    public async Task GetEarthQuakeInfo_MapsDetailById()
    {
        var requester = new StubHttpRequester
        {
            Response = "{\"code\":0,\"data\":{\"detail_info\":{" +
                       "\"id\":\"event-1\",\"location\":\"云南\",\"level\":\"4.0\",\"depth\":5," +
                       "\"latitude\":23.649,\"longitude\":102.253,\"created_at\":200}}}"
        };
        var api = new CencEarthQuakeApi(requester, new JsonConvertService(),
            NullLogger<CencEarthQuakeApi>.Instance);

        var values = await api.GetEarthQuakeInfo("event-1", CancellationToken.None);

        values.Should().ContainSingle().Which.Id.Should().Be("event-1");
        requester.LastUrl.Should().Contain("id=event-1");
        requester.LastUrl.Should().Contain("&v=");
    }

    [TestMethod]
    public async Task RapidApi_RequestsRapidReports()
    {
        var requester = new StubHttpRequester
        {
            Response = "{\"code\":0,\"data\":{\"spot_infos\":[]}}"
        };
        var api = new CencRapidEarthQuakeApi(requester, new JsonConvertService(),
            NullLogger<CencEarthQuakeApi>.Instance);

        await api.GetEarthQuakeList(1, CancellationToken.None);

        requester.LastUrl.Should().Contain("alarm_type=2");
    }

    private sealed class StubHttpRequester : IHttpRequester
    {
        public string Response { get; init; } = string.Empty;
        public string LastUrl { get; private set; } = string.Empty;

        public Task<string> GetString(string url, Dictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            LastUrl = url + (data is null ? string.Empty : "?" +
                string.Join("&", data.Select(pair => $"{pair.Key}={pair.Value}")));
            return Task.FromResult(Response);
        }

        public Task<string> PostString(string url, System.Net.Http.HttpContent? data = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeWaring.App.Services.EarthQuakeSources;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EarthquakeWaring.App.Tests;

[TestClass]
public sealed class WolfxHttpQuerySourceTests
{
    [TestMethod]
    public async Task GetRecentAsync_CombinesCencAndJmaHttpLists()
    {
        var handler = new StubHttpHandler(request => request.RequestUri!.AbsolutePath switch
        {
            "/cenc_eqlist.json" =>
                "{\"No1\":{\"EventID\":\"C1\",\"time\":\"2026-08-27 10:00:00\"," +
                "\"location\":\"四川\",\"magnitude\":\"3\",\"depth\":\"8\"," +
                "\"latitude\":\"30\",\"longitude\":\"104\"}}",
            "/jma_eqlist.json" =>
                "{\"No1\":{\"EventID\":\"J1\",\"time\":\"2026/08/27 11:00\"," +
                "\"location\":\"日本\",\"magnitude\":\"4\",\"depth\":\"10km\"," +
                "\"latitude\":\"35\",\"longitude\":\"139\"}}",
            _ => "{}"
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.wolfx.jp/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(value => value.CreateClient("Wolfx")).Returns(client);
        var source = new WolfxHttpQuerySource(
            factory.Object,
            new WolfxMessageParser(),
            new WolfxReportCache(),
            NullLogger<WolfxHttpQuerySource>.Instance);

        var reports = await source.GetRecentAsync(CancellationToken.None);

        reports.Should().HaveCount(2);
        reports.Should().Contain(report => report.EventKey.Provider == "cenc_eqlist");
        reports.Should().Contain(report => report.EventKey.Provider == "jma_eqlist");
        factory.Verify(value => value.CreateClient("Wolfx"), Times.Exactly(2));
    }

    private sealed class StubHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string> _responseFactory;

        public StubHttpHandler(Func<HttpRequestMessage, string> responseFactory) =>
            _responseFactory = responseFactory;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_responseFactory(request))
        });
    }
}

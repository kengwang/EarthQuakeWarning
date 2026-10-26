using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarthquakeWaring.App.Tests;

[TestClass]
public sealed class ServiceRegistrationTests
{
    [TestMethod]
    public void ConfigureServices_ResolvesEverySelectableSource()
    {
        var services = new ServiceCollection();
        App.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        var catalog = provider.GetRequiredService<IEarthquakeSourceCatalog>();

        foreach (var sourceId in EarthquakeSourceIds.LegacyOrder)
        {
            catalog.GetRealtimeSource(sourceId).Should().NotBeNull();
            catalog.GetQuerySource(sourceId).Should().NotBeNull();
        }
        catalog.GetRealtimeSource(EarthquakeSourceIds.WolfxWebSocket).Should().NotBeNull();
        catalog.GetQuerySource(EarthquakeSourceIds.WolfxWebSocket).Should().NotBeNull();
    }
}

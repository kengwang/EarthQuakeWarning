using System.Text.Json;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarthquakeWaring.App.Tests;

[TestClass]
public sealed class UpdaterSettingTests
{
    [TestMethod]
    public void Deserialize_LegacyApiType_MapsToStableSourceId()
    {
        var setting = JsonSerializer.Deserialize<UpdaterSetting>("{\"ApiType\":3}");

        setting.Should().NotBeNull();
        setting!.SourceId.Should().Be(EarthquakeSourceIds.WolfxCencHttp);
    }

    [TestMethod]
    public void Serialize_MigratedSetting_WritesSourceIdWithoutApiType()
    {
        var setting = JsonSerializer.Deserialize<UpdaterSetting>("{\"ApiType\":2}")!;
        _ = setting.SourceId;

        var json = JsonSerializer.Serialize(setting);

        json.Should().Contain("\"SourceId\":\"wolfx-sceew-http\"");
        json.Should().NotContain("\"ApiType\"");
    }
}

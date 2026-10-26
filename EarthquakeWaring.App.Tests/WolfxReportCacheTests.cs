using System;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Services.EarthQuakeSources;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarthquakeWaring.App.Tests;

[TestClass]
public sealed class WolfxReportCacheTests
{
    [TestMethod]
    public void Add_DuplicateReportNumber_StoresOneReport()
    {
        var cache = new WolfxReportCache();
        var key = new EarthquakeEventKey("sc_eew", "1");
        var first = CreateReport(key, 1, DateTimeOffset.Parse("2026-08-27T10:00:00+08:00"));
        var duplicate = first with { Magnitude = 6 };

        cache.Add(first);
        cache.Add(duplicate);

        cache.Get(key).Should().ContainSingle().Which.Magnitude.Should().Be(5);
    }

    [TestMethod]
    public void Add_DifferentReports_ReturnsThemInTimeOrder()
    {
        var cache = new WolfxReportCache();
        var key = new EarthquakeEventKey("sc_eew", "1");
        var later = CreateReport(key, 2, DateTimeOffset.Parse("2026-08-27T10:00:02+08:00"));
        var earlier = CreateReport(key, 1, DateTimeOffset.Parse("2026-08-27T10:00:01+08:00"));

        cache.Add(later);
        cache.Add(earlier);

        cache.Get(key).Should().ContainInOrder(earlier, later);
    }

    private static EarthquakeReport CreateReport(EarthquakeEventKey key, int number, DateTimeOffset reportTime) =>
        new(key, reportTime.AddSeconds(-1), reportTime, number, 30, 104, 5, 10, "test", EarthquakeReportStatus.Normal);
}

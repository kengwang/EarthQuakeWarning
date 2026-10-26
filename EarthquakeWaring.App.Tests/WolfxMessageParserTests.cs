using System;
using System.Text.Json;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Services.EarthQuakeSources;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarthquakeWaring.App.Tests;

[TestClass]
public sealed class WolfxMessageParserTests
{
    private readonly WolfxMessageParser _parser = new();

    [DataTestMethod]
    [DataRow("sc_eew", "Magunitude")]
    [DataRow("fj_eew", "Magunitude")]
    [DataRow("cq_eew", "Magnitude")]
    [DataRow("cenc_eew", "Magnitude")]
    public void ParseWebSocket_ChineseEew_MapsNormalizedReport(string type, string magnitudeField)
    {
        var json = "{\"type\":\"" + type + "\",\"ID\":1,\"EventID\":\"event-1\"," +
                   "\"ReportTime\":\"2026-08-27 10:01:02\",\"ReportNum\":2," +
                   "\"OriginTime\":\"2026-08-27 10:00:00\",\"HypoCenter\":\"测试地点\"," +
                   "\"Latitude\":30.1,\"Longitude\":104.2,\"" + magnitudeField + "\":5.3,\"Depth\":10}";

        var result = _parser.ParseWebSocket(json);

        result.IsHistory.Should().BeFalse();
        result.Reports.Should().ContainSingle();
        result.Reports[0].EventKey.Should().Be(new EarthquakeEventKey(type, "event-1"));
        result.Reports[0].ReportNumber.Should().Be(2);
        result.Reports[0].Magnitude.Should().Be(5.3);
        result.Reports[0].Depth.Should().Be(10);
    }

    [TestMethod]
    public void ParseWebSocket_JmaTrainingReport_IsMarkedAsTraining()
    {
        const string json = "{\"type\":\"jma_eew\",\"EventID\":\"jma-1\",\"Serial\":3," +
                            "\"AnnouncedTime\":\"2026/08/27 10:01:00\",\"OriginTime\":\"2026/08/27 10:00:00\"," +
                            "\"Hypocenter\":\"测试\",\"Latitude\":30,\"Longitude\":130,\"Magunitude\":4.1," +
                            "\"Depth\":20,\"isTraining\":true,\"isCancel\":false,\"isFinal\":false}";

        var result = _parser.ParseWebSocket(json);

        result.Reports.Should().ContainSingle().Which.Status.Should().Be(EarthquakeReportStatus.Training);
    }

    [TestMethod]
    public void ParseWebSocket_FinalAndCancelledReports_MapLifecycleStatus()
    {
        const string finalJson = "{\"type\":\"fj_eew\",\"EventID\":\"fj-1\"," +
                                 "\"ReportTime\":\"2026-08-27 10:01:00\",\"OriginTime\":\"2026-08-27 10:00:00\"," +
                                 "\"Latitude\":25,\"Longitude\":118,\"Magunitude\":4,\"isFinal\":true}";
        const string cancelledJson = "{\"type\":\"jma_eew\",\"EventID\":\"jma-2\",\"Serial\":4," +
                                     "\"AnnouncedTime\":\"2026/08/27 10:01:00\",\"OriginTime\":\"2026/08/27 10:00:00\"," +
                                     "\"Latitude\":30,\"Longitude\":130,\"Magunitude\":4,\"isCancel\":true}";

        var final = _parser.ParseWebSocket(finalJson);
        var cancelled = _parser.ParseWebSocket(cancelledJson);

        final.Reports.Should().ContainSingle().Which.Status.Should().Be(EarthquakeReportStatus.Final);
        cancelled.Reports.Should().ContainSingle().Which.Status.Should().Be(EarthquakeReportStatus.Cancelled);
    }

    [TestMethod]
    public void ParseWebSocket_UnknownType_ReturnsNoReports()
    {
        var result = _parser.ParseWebSocket("{\"type\":\"future_type\"}");

        result.Reports.Should().BeEmpty();
        result.ShouldReplyPing.Should().BeFalse();
        result.IsHistory.Should().BeFalse();
    }

    [TestMethod]
    public void ParseWebSocket_Heartbeat_RequestsPingWithoutReport()
    {
        var result = _parser.ParseWebSocket("{\"type\":\"heartbeat\",\"timestamp\":\"1\"}");

        result.ShouldReplyPing.Should().BeTrue();
        result.Reports.Should().BeEmpty();
    }

    [TestMethod]
    public void ParseWebSocket_HistoryList_ParsesNoPrefixedEntriesWithoutAlerting()
    {
        const string json = "{\"type\":\"cenc_eqlist\",\"No1\":{\"EventID\":\"CC.1\"," +
                            "\"time\":\"2026-08-27 10:00:00\",\"ReportTime\":\"2026-08-27 10:01:00\"," +
                            "\"location\":\"四川\",\"magnitude\":\"3.3\",\"depth\":\"8\"," +
                            "\"latitude\":\"28.52\",\"longitude\":\"104.67\"},\"md5\":\"x\"}";

        var result = _parser.ParseWebSocket(json);

        result.IsHistory.Should().BeTrue();
        result.Reports.Should().ContainSingle().Which.EventKey
            .Should().Be(new EarthquakeEventKey("cenc_eqlist", "CC.1"));
    }

    [TestMethod]
    public void ParseWebSocket_MalformedJson_ThrowsJsonException()
    {
        Action act = () => _parser.ParseWebSocket("not-json");

        act.Should().Throw<JsonException>();
    }
}

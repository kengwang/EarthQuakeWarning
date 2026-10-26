using System;

namespace EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

/// <summary>
/// Identifies an earthquake event within the namespace of one upstream provider.
/// </summary>
/// <param name="Provider">The stable provider identifier.</param>
/// <param name="EventId">The provider-specific event identifier.</param>
public readonly record struct EarthquakeEventKey(string Provider, string EventId);

/// <summary>
/// Describes the lifecycle state of an earthquake report.
/// </summary>
public enum EarthquakeReportStatus
{
    /// <summary>A regular, active report.</summary>
    Normal,
    /// <summary>The provider marked this as the final report.</summary>
    Final,
    /// <summary>The provider cancelled the event.</summary>
    Cancelled,
    /// <summary>The report is a training or drill message.</summary>
    Training
}

/// <summary>
/// Represents a transport-independent earthquake report.
/// </summary>
/// <param name="EventKey">The provider-qualified event key.</param>
/// <param name="OriginTime">The earthquake origin time.</param>
/// <param name="ReportTime">The time at which this report was issued.</param>
/// <param name="ReportNumber">The provider report sequence number, when available.</param>
/// <param name="Latitude">The hypocentre latitude.</param>
/// <param name="Longitude">The hypocentre longitude.</param>
/// <param name="Magnitude">The reported magnitude.</param>
/// <param name="Depth">The hypocentre depth in kilometres, when available.</param>
/// <param name="Hypocenter">The human-readable hypocentre name.</param>
/// <param name="Status">The lifecycle state of the report.</param>
public sealed record EarthquakeReport(
    EarthquakeEventKey EventKey,
    DateTimeOffset OriginTime,
    DateTimeOffset ReportTime,
    int? ReportNumber,
    double Latitude,
    double Longitude,
    double Magnitude,
    double? Depth,
    string? Hypocenter,
    EarthquakeReportStatus Status)
{
    /// <summary>Converts this report to the legacy UI model.</summary>
    /// <returns>A populated legacy earthquake model.</returns>
    public EarthQuakeInfoBase ToLegacyModel() => new()
    {
        Id = EventKey.EventId,
        Provider = EventKey.Provider,
        StartAt = OriginTime.LocalDateTime,
        UpdateAt = ReportTime.LocalDateTime,
        Latitude = Latitude,
        Longitude = Longitude,
        Magnitude = Magnitude,
        Depth = Depth ?? 0,
        PlaceName = Hypocenter
    };

    /// <summary>Creates a report from the legacy API model.</summary>
    /// <param name="provider">The stable provider identifier.</param>
    /// <param name="value">The legacy model to convert.</param>
    /// <returns>A transport-independent report.</returns>
    public static EarthquakeReport FromLegacyModel(string provider, EarthQuakeInfoBase value) => new(
        new EarthquakeEventKey(provider, value.Id),
        ToOffset(value.StartAt),
        ToOffset(value.UpdateAt == default ? value.StartAt : value.UpdateAt),
        null,
        value.Latitude,
        value.Longitude,
        value.Magnitude,
        value.Depth,
        value.PlaceName,
        EarthquakeReportStatus.Normal);

    private static DateTimeOffset ToOffset(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => new DateTimeOffset(value, TimeSpan.Zero),
        DateTimeKind.Local => new DateTimeOffset(value),
        _ => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Local))
    };
}

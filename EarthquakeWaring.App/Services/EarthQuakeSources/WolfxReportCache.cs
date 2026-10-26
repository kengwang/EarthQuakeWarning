using System;
using System.Collections.Generic;
using System.Linq;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Services.EarthQuakeSources;

/// <summary>Stores a bounded in-memory sequence of Wolfx reports received during this application session.</summary>
public interface IWolfxReportCache
{
    /// <summary>Adds a report to the cache.</summary>
    /// <param name="report">The report to store.</param>
    void Add(EarthquakeReport report);

    /// <summary>Gets cached reports for one event.</summary>
    /// <param name="eventKey">The provider-qualified event key.</param>
    /// <returns>Cached reports ordered by report time.</returns>
    IReadOnlyList<EarthquakeReport> Get(EarthquakeEventKey eventKey);
}

/// <inheritdoc />
public sealed class WolfxReportCache : IWolfxReportCache
{
    private const int MaximumEvents = 100;
    private const int MaximumReportsPerEvent = 100;
    private readonly object _gate = new();
    private readonly Dictionary<EarthquakeEventKey, List<EarthquakeReport>> _reports = new();
    private readonly Queue<EarthquakeEventKey> _eventOrder = new();

    /// <inheritdoc />
    public void Add(EarthquakeReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        lock (_gate)
        {
            if (!_reports.TryGetValue(report.EventKey, out var eventReports))
            {
                eventReports = new List<EarthquakeReport>();
                _reports.Add(report.EventKey, eventReports);
                _eventOrder.Enqueue(report.EventKey);
                while (_reports.Count > MaximumEvents && _eventOrder.TryDequeue(out var oldest))
                    _reports.Remove(oldest);
            }

            if (eventReports.Any(existing => IsSameReport(existing, report)))
                return;

            eventReports.Add(report);
            eventReports.Sort((left, right) => left.ReportTime.CompareTo(right.ReportTime));
            if (eventReports.Count > MaximumReportsPerEvent)
                eventReports.RemoveRange(0, eventReports.Count - MaximumReportsPerEvent);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<EarthquakeReport> Get(EarthquakeEventKey eventKey)
    {
        lock (_gate)
            return _reports.TryGetValue(eventKey, out var reports) ? reports.ToArray() : Array.Empty<EarthquakeReport>();
    }

    private static bool IsSameReport(EarthquakeReport left, EarthquakeReport right) =>
        left.ReportNumber.HasValue && right.ReportNumber.HasValue
            ? left.ReportNumber == right.ReportNumber
            : left.ReportTime == right.ReportTime;
}

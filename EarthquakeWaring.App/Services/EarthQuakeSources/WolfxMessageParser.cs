using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Services.EarthQuakeSources;

/// <summary>Represents the routing result of one Wolfx JSON message.</summary>
public sealed record WolfxMessageParseResult(
    string MessageType,
    IReadOnlyList<EarthquakeReport> Reports,
    bool ShouldReplyPing,
    bool IsHistory);

/// <summary>Parses Wolfx HTTP and WebSocket payloads into normalized reports.</summary>
public sealed class WolfxMessageParser
{
    private static readonly TimeSpan ChinaOffset = TimeSpan.FromHours(8);
    private static readonly TimeSpan JapanOffset = TimeSpan.FromHours(9);

    /// <summary>Parses a WebSocket payload using its root <c>type</c> discriminator.</summary>
    /// <param name="json">The UTF-8 JSON payload.</param>
    /// <returns>The parsed routing result.</returns>
    /// <exception cref="JsonException">Thrown when the payload is invalid or has no type.</exception>
    public WolfxMessageParseResult ParseWebSocket(string json)
    {
        using var document = JsonDocument.Parse(json);
        var type = GetString(document.RootElement, "type") ?? throw new JsonException("Wolfx message has no type.");
        return ParseRoot(document.RootElement, type);
    }

    /// <summary>Parses an HTTP payload whose message type is implied by its endpoint.</summary>
    /// <param name="json">The UTF-8 JSON payload.</param>
    /// <param name="messageType">The corresponding Wolfx message type.</param>
    /// <returns>The parsed routing result.</returns>
    public WolfxMessageParseResult ParseHttp(string json, string messageType)
    {
        using var document = JsonDocument.Parse(json);
        return ParseRoot(document.RootElement, messageType);
    }

    private static WolfxMessageParseResult ParseRoot(JsonElement root, string type) => type switch
    {
        "heartbeat" => new WolfxMessageParseResult(type, Array.Empty<EarthquakeReport>(), true, false),
        "pong" => new WolfxMessageParseResult(type, Array.Empty<EarthquakeReport>(), false, false),
        "sc_eew" or "fj_eew" or "cq_eew" or "cenc_eew" =>
            Single(type, ParseChinaEew(root, type), false),
        "jma_eew" => Single(type, ParseJmaEew(root), false),
        "cenc_eqlist" => new WolfxMessageParseResult(type, ParseList(root, type, ChinaOffset), false, true),
        "jma_eqlist" => new WolfxMessageParseResult(type, ParseList(root, type, JapanOffset), false, true),
        _ => new WolfxMessageParseResult(type, Array.Empty<EarthquakeReport>(), false, false)
    };

    private static WolfxMessageParseResult Single(string type, EarthquakeReport? report, bool history) =>
        new(type, report is null ? Array.Empty<EarthquakeReport>() : new[] { report }, false, history);

    private static EarthquakeReport? ParseChinaEew(JsonElement root, string provider)
    {
        var eventId = GetString(root, "EventID") ?? GetString(root, "ID");
        if (string.IsNullOrWhiteSpace(eventId))
            return null;

        var origin = GetDate(root, "OriginTime", ChinaOffset);
        var report = GetDate(root, "ReportTime", ChinaOffset);
        if (origin is null || report is null)
            return null;

        var status = GetBoolean(root, "isCancel") == true
            ? EarthquakeReportStatus.Cancelled
            : GetBoolean(root, "isFinal") == true
                ? EarthquakeReportStatus.Final
                : EarthquakeReportStatus.Normal;

        return new EarthquakeReport(
            new EarthquakeEventKey(provider, eventId),
            origin.Value,
            report.Value,
            GetInt32(root, "ReportNum"),
            GetDouble(root, "Latitude") ?? 0,
            GetDouble(root, "Longitude") ?? 0,
            GetDouble(root, "Magnitude") ?? GetDouble(root, "Magunitude") ?? 0,
            GetDouble(root, "Depth"),
            GetString(root, "HypoCenter"),
            status);
    }

    private static EarthquakeReport? ParseJmaEew(JsonElement root)
    {
        var eventId = GetString(root, "EventID");
        var origin = GetDate(root, "OriginTime", JapanOffset);
        var report = GetDate(root, "AnnouncedTime", JapanOffset);
        if (string.IsNullOrWhiteSpace(eventId) || origin is null || report is null)
            return null;

        var status = GetBoolean(root, "isTraining") == true
            ? EarthquakeReportStatus.Training
            : GetBoolean(root, "isCancel") == true
                ? EarthquakeReportStatus.Cancelled
                : GetBoolean(root, "isFinal") == true
                    ? EarthquakeReportStatus.Final
                    : EarthquakeReportStatus.Normal;

        return new EarthquakeReport(
            new EarthquakeEventKey("jma_eew", eventId),
            origin.Value,
            report.Value,
            GetInt32(root, "Serial"),
            GetDouble(root, "Latitude") ?? 0,
            GetDouble(root, "Longitude") ?? 0,
            GetDouble(root, "Magunitude") ?? GetDouble(root, "Magnitude") ?? 0,
            GetDouble(root, "Depth"),
            GetString(root, "Hypocenter"),
            status);
    }

    private static IReadOnlyList<EarthquakeReport> ParseList(JsonElement root, string provider, TimeSpan offset)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return Array.Empty<EarthquakeReport>();

        var reports = new List<EarthquakeReport>();
        foreach (var property in root.EnumerateObject())
        {
            var ordinalText = property.Name.StartsWith("No", StringComparison.OrdinalIgnoreCase)
                ? property.Name[2..]
                : property.Name;
            if (property.Value.ValueKind != JsonValueKind.Object || !int.TryParse(ordinalText, out _))
                continue;

            var item = property.Value;
            var origin = GetDate(item, "time", offset);
            if (origin is null)
                continue;
            var reportTime = GetDate(item, "ReportTime", offset) ??
                             GetDate(item, "time_full", offset) ?? origin.Value;

            var eventId = GetString(item, "EventID") ??
                          $"{origin.Value.ToUnixTimeMilliseconds()}:{GetString(item, "latitude")}:{GetString(item, "longitude")}";
            reports.Add(new EarthquakeReport(
                new EarthquakeEventKey(provider, eventId),
                origin.Value,
                reportTime,
                null,
                GetDouble(item, "latitude") ?? 0,
                GetDouble(item, "longitude") ?? 0,
                GetDouble(item, "magnitude") ?? 0,
                GetDepth(item),
                GetString(item, "location") ?? GetString(item, "placeName"),
                EarthquakeReportStatus.Normal));
        }

        return reports.OrderByDescending(value => value.OriginTime).ToList();
    }

    private static double? GetDepth(JsonElement element)
    {
        var numeric = GetDouble(element, "depth");
        if (numeric.HasValue)
            return numeric;
        var text = GetString(element, "depth");
        if (text is null)
            return null;
        var digits = new string(text.Where(character => char.IsDigit(character) || character is '.' or '-').ToArray());
        return double.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
    }

    private static double? GetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var numeric))
            return numeric;
        return double.TryParse(property.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out numeric)
            ? numeric
            : null;
    }

    private static int? GetInt32(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var numeric))
            return numeric;
        return int.TryParse(property.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numeric)
            ? numeric
            : null;
    }

    private static bool? GetBoolean(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;
        if (property.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return property.GetBoolean();
        return bool.TryParse(property.ToString(), out var value) ? value : null;
    }

    private static DateTimeOffset? GetDate(JsonElement element, string propertyName, TimeSpan defaultOffset)
    {
        var value = GetString(element, propertyName);
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (long.TryParse(value, out var timestamp))
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed))
            return null;
        return parsed.Kind switch
        {
            DateTimeKind.Utc => new DateTimeOffset(parsed, TimeSpan.Zero),
            DateTimeKind.Local => new DateTimeOffset(parsed),
            _ => new DateTimeOffset(parsed, defaultOffset)
        };
    }
}

using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EarthquakeWaring.App.Services;

public class JsonConvertService : IJsonConvertService
{
    public static readonly JsonSerializerOptions? JsonSerializerOptions = new JsonSerializerOptions
    {
        Converters = { new JsonNumberToDatetimeConverter() }
    };

    public T? ConvertTo<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }

    public string ConvertBack<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }
}

public class JsonNumberToDatetimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64()).LocalDateTime;
        throw new JsonException("Unexpected token type within NumberToStringConverter");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToFileTime());
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Max.Bot.Types.Converters;

/// <summary>
/// JSON converter for Unix timestamp to DateTime conversion.
/// </summary>
public class UnixTimestampJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Reads a Unix timestamp (as a number) and converts it to a DateTime.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A DateTime converted from the Unix timestamp.</returns>
    /// <exception cref="JsonException">Thrown when the token type is not a number.</exception>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected Number.");
    }

    /// <summary>
    /// Writes a DateTime as a Unix timestamp (as a number).
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The DateTime value to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Convert DateTime to DateTimeOffset for Unix timestamp conversion
        // DateTimeOffset requires the DateTime to be within a valid range (year 0-9999)
        // If the DateTime.Kind is Unspecified, assume UTC
        var dateTimeOffset = value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(value, TimeSpan.Zero)
            : new DateTimeOffset(value);

        var unixTime = dateTimeOffset.ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}


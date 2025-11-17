using System.Text.Json;
using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types.Converters;
using Xunit;

namespace Max.Bot.Tests.Unit.Types.Converters;

public class UnixTimestampJsonConverterTests
{
    [Theory]
    [InlineData(0, "1970-01-01T00:00:00Z")]
    [InlineData(1609459200, "2021-01-01T00:00:00Z")] // 2021-01-01 00:00:00 UTC
    [InlineData(1640995200, "2022-01-01T00:00:00Z")] // 2022-01-01 00:00:00 UTC
    public void Read_ShouldConvertUnixTimestampToDateTime(long unixTimestamp, string expectedDateTimeString)
    {
        // Arrange
        var expectedDateTime = DateTime.Parse(expectedDateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var json = $"{unixTimestamp}";
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var result = JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        result.Should().BeCloseTo(expectedDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenTokenTypeIsNotNumber()
    {
        // Arrange
        var json = "\"invalid\"";
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var act = () => JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*Unexpected token type*");
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00Z", 0)]
    [InlineData("2021-01-01T00:00:00Z", 1609459200)]
    [InlineData("2022-01-01T00:00:00Z", 1640995200)]
    public void Write_ShouldConvertDateTimeToUnixTimestamp(string dateTimeString, long expectedUnixTimestamp)
    {
        // Arrange
        var dateTime = DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var json = JsonSerializer.Serialize(dateTime, options);

        // Assert
        json.Should().Be($"{expectedUnixTimestamp}");
    }

    [Fact]
    public void Write_ShouldHandleUtcDateTime()
    {
        // Arrange
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var json = JsonSerializer.Serialize(dateTime, options);

        // Assert
        json.Should().NotBeNullOrEmpty();
        var unixTimestamp = long.Parse(json);
        unixTimestamp.Should().Be(0);
    }

    [Fact]
    public void Write_ShouldHandleUnspecifiedKind()
    {
        // Arrange
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var json = JsonSerializer.Serialize(dateTime, options);

        // Assert
        json.Should().NotBeNullOrEmpty();
        var unixTimestamp = long.Parse(json);
        unixTimestamp.Should().Be(0);
    }

    [Fact]
    public void Write_ShouldHandleMaxValue()
    {
        // Arrange
        var dateTime = DateTime.MaxValue;
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var json = JsonSerializer.Serialize(dateTime, options);

        // Assert
        json.Should().NotBeNullOrEmpty();
        var unixTimestamp = long.Parse(json);
        unixTimestamp.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveDateTime()
    {
        // Arrange
        var originalDateTime = new DateTime(2021, 6, 15, 12, 30, 45, DateTimeKind.Utc);
        var options = new JsonSerializerOptions { Converters = { new UnixTimestampJsonConverter() } };

        // Act
        var json = JsonSerializer.Serialize(originalDateTime, options);
        var deserializedDateTime = JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        deserializedDateTime.Should().BeCloseTo(originalDateTime, TimeSpan.FromSeconds(1));
    }
}


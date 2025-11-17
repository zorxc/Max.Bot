using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types.Enums;

public class MessageTypeTests
{
    [Theory]
    [InlineData("text", MessageType.Text)]
    [InlineData("image", MessageType.Image)]
    [InlineData("file", MessageType.File)]
    public void Deserialize_ShouldParseJsonString(string jsonValue, MessageType expected)
    {
        // Arrange
        var json = $"\"{jsonValue}\"";

        // Act
        var result = MaxJsonSerializer.Deserialize<MessageType>(json);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MessageType.Text, "text")]
    [InlineData(MessageType.Image, "image")]
    [InlineData(MessageType.File, "file")]
    public void Serialize_ShouldConvertToJsonString(MessageType value, string expectedJsonValue)
    {
        // Arrange
        var expectedJson = $"\"{expectedJsonValue}\"";

        // Act
        var json = MaxJsonSerializer.Serialize(value);

        // Assert
        json.Should().Be(expectedJson);
    }

    [Fact]
    public void Serialize_ShouldHandleAllValues()
    {
        // Arrange
        var values = Enum.GetValues<MessageType>();

        // Act & Assert
        foreach (var value in values)
        {
            var json = MaxJsonSerializer.Serialize(value);
            json.Should().NotBeNullOrEmpty();
            json.Should().StartWith("\"").And.EndWith("\"");
        }
    }
}


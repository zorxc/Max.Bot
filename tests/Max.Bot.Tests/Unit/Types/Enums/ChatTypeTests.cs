using System.Text.Json;
using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types.Enums;

public class ChatTypeTests
{
    [Theory]
    [InlineData("dialog", ChatType.Dialog)]
    [InlineData("chat", ChatType.Chat)]
    [InlineData("channel", ChatType.Channel)]
    public void Deserialize_ShouldParseJsonString(string jsonValue, ChatType expected)
    {
        // Arrange
        var json = $"\"{jsonValue}\"";

        // Act
        var result = MaxJsonSerializer.Deserialize<ChatType>(json);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ChatType.Dialog, "dialog")]
    [InlineData(ChatType.Chat, "chat")]
    [InlineData(ChatType.Channel, "channel")]
    public void Serialize_ShouldConvertToJsonString(ChatType value, string expectedJsonValue)
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
        var values = Enum.GetValues<ChatType>();

        // Act & Assert
        foreach (var value in values)
        {
            var json = MaxJsonSerializer.Serialize(value);
            json.Should().NotBeNullOrEmpty();
            json.Should().StartWith("\"").And.EndWith("\"");
        }
    }
}


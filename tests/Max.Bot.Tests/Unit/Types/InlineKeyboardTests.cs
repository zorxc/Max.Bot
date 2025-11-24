using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class InlineKeyboardTests
{
    [Fact]
    public void InlineKeyboard_ShouldDeserialize_FromJson_WithNewFormat()
    {
        // Arrange
        var json = """{"buttons":[[{"type":"callback","text":"Button 1","payload":"callback1"}],[{"type":"link","text":"Button 2","url":"https://example.com"}]]}""";

        // Act
        var keyboard = MaxJsonSerializer.Deserialize<InlineKeyboard>(json);

        // Assert
        keyboard.Should().NotBeNull();
        keyboard.Buttons.Should().HaveCount(2);
        keyboard.Buttons[0].Should().HaveCount(1);
        keyboard.Buttons[0][0].Type.Should().Be(ButtonType.Callback);
        keyboard.Buttons[0][0].Text.Should().Be("Button 1");
        keyboard.Buttons[0][0].Payload.Should().Be("callback1");
        keyboard.Buttons[1].Should().HaveCount(1);
        keyboard.Buttons[1][0].Type.Should().Be(ButtonType.Link);
        keyboard.Buttons[1][0].Text.Should().Be("Button 2");
        keyboard.Buttons[1][0].Url.Should().Be("https://example.com");
    }

    [Fact]
    public void InlineKeyboard_ShouldDeserialize_FromJson_WithLegacyFormat()
    {
        // Arrange - legacy format for backward compatibility
        var json = """{"buttons":[[{"text":"Button 1","callback_data":"callback1"}],[{"text":"Button 2","url":"https://example.com"}]]}""";

        // Act
        var keyboard = MaxJsonSerializer.Deserialize<InlineKeyboard>(json);

        // Assert
        keyboard.Should().NotBeNull();
        keyboard.Buttons.Should().HaveCount(2);
        keyboard.Buttons[0].Should().HaveCount(1);
        keyboard.Buttons[0][0].Type.Should().Be(ButtonType.Callback);
        keyboard.Buttons[0][0].Text.Should().Be("Button 1");
        keyboard.Buttons[0][0].Payload.Should().Be("callback1");
        keyboard.Buttons[1].Should().HaveCount(1);
        keyboard.Buttons[1][0].Text.Should().Be("Button 2");
        keyboard.Buttons[1][0].Url.Should().Be("https://example.com");
    }

    [Fact]
    public void InlineKeyboard_ShouldDeserialize_WithEmptyButtons()
    {
        // Arrange
        var json = """{"buttons":[]}""";

        // Act
        var keyboard = MaxJsonSerializer.Deserialize<InlineKeyboard>(json);

        // Assert
        keyboard.Should().NotBeNull();
        keyboard.Buttons.Should().BeEmpty();
    }

    [Fact]
    public void InlineKeyboard_ShouldSerialize_ToJson_WithNewFormat()
    {
        // Arrange
        var keyboard = new InlineKeyboard
        {
            Buttons = new[]
            {
                new[]
                {
                    new InlineKeyboardButton
                    {
                        Type = ButtonType.Callback,
                        Text = "Button 1",
                        Payload = "callback1"
                    }
                },
                new[]
                {
                    new InlineKeyboardButton
                    {
                        Type = ButtonType.Link,
                        Text = "Button 2",
                        Url = "https://example.com"
                    }
                }
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize(keyboard);

        // Assert
        json.Should().Contain("\"buttons\"");
        json.Should().Contain("\"type\":\"callback\"");
        json.Should().Contain("\"text\":\"Button 1\"");
        json.Should().Contain("\"payload\":\"callback1\"");
        json.Should().Contain("\"type\":\"link\"");
        json.Should().Contain("\"text\":\"Button 2\"");
        json.Should().Contain("\"url\":\"https://example.com\"");
    }

    [Fact]
    public void InlineKeyboard_ShouldSerialize_UsingCallbackData()
    {
        // Arrange - using CallbackData for backward compatibility
        var keyboard = new InlineKeyboard
        {
            Buttons = new[]
            {
                new[]
                {
                    new InlineKeyboardButton { Text = "Button 1", CallbackData = "callback1" }
                }
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize(keyboard);

        // Assert
        json.Should().Contain("\"buttons\"");
        json.Should().Contain("\"type\":\"callback\"");
        json.Should().Contain("\"text\":\"Button 1\"");
        json.Should().Contain("\"payload\":\"callback1\"");
        keyboard.Buttons[0][0].Type.Should().Be(ButtonType.Callback);
    }

    [Fact]
    public void InlineKeyboard_ShouldSerialize_WithEmptyButtons()
    {
        // Arrange
        var keyboard = new InlineKeyboard
        {
            Buttons = Array.Empty<InlineKeyboardButton[]>()
        };

        // Act
        var json = MaxJsonSerializer.Serialize(keyboard);

        // Assert
        json.Should().Contain("\"buttons\":[]");
    }
}


// 📁 [InlineKeyboardButtonTests] - Тесты для модели InlineKeyboardButton
// 🎯 Core function: Тестирование сериализации/десериализации InlineKeyboardButton
// 🔗 Key dependencies: Max.Bot.Types, Max.Bot.Networking, FluentAssertions, xUnit
// 💡 Usage: Unit тесты для проверки корректности работы модели InlineKeyboardButton

using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class InlineKeyboardButtonTests
{
    [Fact]
    public void InlineKeyboardButton_ShouldDeserialize_FromJson_WithCallbackData()
    {
        // Arrange
        var json = """{"text":"Button Text","callbackData":"callback123"}""";

        // Act
        var button = MaxJsonSerializer.Deserialize<InlineKeyboardButton>(json);

        // Assert
        button.Should().NotBeNull();
        button.Text.Should().Be("Button Text");
        button.CallbackData.Should().Be("callback123");
        button.Url.Should().BeNull();
    }

    [Fact]
    public void InlineKeyboardButton_ShouldDeserialize_FromJson_WithUrl()
    {
        // Arrange
        var json = """{"text":"Open URL","url":"https://example.com"}""";

        // Act
        var button = MaxJsonSerializer.Deserialize<InlineKeyboardButton>(json);

        // Assert
        button.Should().NotBeNull();
        button.Text.Should().Be("Open URL");
        button.Url.Should().Be("https://example.com");
        button.CallbackData.Should().BeNull();
    }

    [Fact]
    public void InlineKeyboardButton_ShouldSerialize_ToJson()
    {
        // Arrange
        var button = new InlineKeyboardButton
        {
            Text = "Button Text",
            CallbackData = "callback123"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(button);

        // Assert
        json.Should().Contain("\"text\":\"Button Text\"");
        json.Should().Contain("\"callbackData\":\"callback123\"");
    }

    [Fact]
    public void InlineKeyboardButton_ShouldSerialize_WithUrl()
    {
        // Arrange
        var button = new InlineKeyboardButton
        {
            Text = "Open URL",
            Url = "https://example.com"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(button);

        // Assert
        json.Should().Contain("\"text\":\"Open URL\"");
        json.Should().Contain("\"url\":\"https://example.com\"");
    }
}


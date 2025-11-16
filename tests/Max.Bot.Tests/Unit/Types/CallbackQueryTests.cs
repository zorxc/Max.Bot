// 📁 [CallbackQueryTests] - Тесты для модели CallbackQuery
// 🎯 Core function: Тестирование сериализации/десериализации CallbackQuery
// 🔗 Key dependencies: Max.Bot.Types, Max.Bot.Networking, FluentAssertions, xUnit
// 💡 Usage: Unit тесты для проверки корректности работы модели CallbackQuery

using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class CallbackQueryTests
{
    [Fact]
    public void CallbackQuery_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"id":"callback123","from":{"id":123,"username":"user123","isBot":false},"message":{"id":456,"text":"Test message","date":1234567890},"data":"callbackData123"}""";

        // Act
        var callbackQuery = MaxJsonSerializer.Deserialize<CallbackQuery>(json);

        // Assert
        callbackQuery.Should().NotBeNull();
        callbackQuery.Id.Should().Be("callback123");
        callbackQuery.From.Should().NotBeNull();
        callbackQuery.From.Id.Should().Be(123);
        callbackQuery.From.Username.Should().Be("user123");
        callbackQuery.Message.Should().NotBeNull();
        callbackQuery.Message!.Id.Should().Be(456);
        callbackQuery.Message.Text.Should().Be("Test message");
        callbackQuery.Data.Should().Be("callbackData123");
    }

    [Fact]
    public void CallbackQuery_ShouldDeserialize_WithNullableFields()
    {
        // Arrange
        var json = """{"id":"callback123","from":{"id":123,"username":"user123","isBot":false}}""";

        // Act
        var callbackQuery = MaxJsonSerializer.Deserialize<CallbackQuery>(json);

        // Assert
        callbackQuery.Should().NotBeNull();
        callbackQuery.Id.Should().Be("callback123");
        callbackQuery.From.Should().NotBeNull();
        callbackQuery.From.Id.Should().Be(123);
        callbackQuery.Message.Should().BeNull();
        callbackQuery.Data.Should().BeNull();
    }

    [Fact]
    public void CallbackQuery_ShouldSerialize_ToJson()
    {
        // Arrange
        var callbackQuery = new CallbackQuery
        {
            Id = "callback123",
            From = new User
            {
                Id = 123,
                Username = "user123",
                IsBot = false
            },
            Data = "callbackData123"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(callbackQuery);

        // Assert
        json.Should().Contain("\"id\":\"callback123\"");
        json.Should().Contain("\"from\"");
        json.Should().Contain("\"data\":\"callbackData123\"");
    }
}


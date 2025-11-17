using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class UpdateTests
{
    [Fact]
    public void Deserialize_ShouldDeserializeUpdateWithMessage()
    {
        // Arrange
        var json = """{"update_id":1,"type":"message_created","message":{"id":123,"chat":{"id":456,"type":"private"},"from":{"id":789,"username":"testuser","is_bot":false},"text":"Hello","date":1609459200}}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Update>(json);

        // Assert
        result.Should().NotBeNull();
        result!.UpdateId.Should().Be(1);
        result.Type.Should().Be(UpdateType.Message);
        result.Message.Should().NotBeNull();
        result.Message!.Id.Should().Be(123);
        result.Message.Text.Should().Be("Hello");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeUpdateWithCallbackQuery()
    {
        // Arrange
        var json = """{"update_id":2,"type":"message_callback","callback_query":{"id":"callback123","from":{"id":123,"username":"user123","is_bot":false},"data":"callbackData123"}}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Update>(json);

        // Assert
        result.Should().NotBeNull();
        result!.UpdateId.Should().Be(2);
        result.Type.Should().Be(UpdateType.CallbackQuery);
        result.CallbackQuery.Should().NotBeNull();
        result.CallbackQuery!.Id.Should().Be("callback123");
        result.CallbackQuery.From.Id.Should().Be(123);
        result.CallbackQuery.Data.Should().Be("callbackData123");
    }

    [Fact]
    public void Serialize_ShouldSerializeUpdate()
    {
        // Arrange
        var update = new Update
        {
            UpdateId = 1,
            UpdateTypeRaw = "message_created",
            Message = new Message
            {
                Id = 123,
                Text = "Hello",
                Date = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize(update);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"update_id\":1");
        json.Should().Contain("\"type\":\"message_created\"");
        json.Should().Contain("\"message\"");
        json.Should().Contain("\"id\":123");
    }

    [Fact]
    public void Serialize_ShouldSerializeUpdateWithCallbackQuery()
    {
        // Arrange
        var update = new Update
        {
            UpdateId = 2,
            UpdateTypeRaw = "message_callback",
            CallbackQuery = new CallbackQuery
            {
                Id = "callback123",
                From = new User { Id = 123, Username = "user123", IsBot = false },
                Data = "callbackData123"
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize(update);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"update_id\":2");
        json.Should().Contain("\"type\":\"message_callback\"");
        json.Should().Contain("\"callback_query\"");
        json.Should().Contain("\"id\":\"callback123\"");
    }

    [Fact]
    public void Serialize_ShouldNotIncludeNullMessage()
    {
        // Arrange
        var update = new Update
        {
            UpdateId = 2,
            UpdateTypeRaw = "message_callback",
            Message = null,
            CallbackQuery = null
        };

        // Act
        var json = MaxJsonSerializer.Serialize(update);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"update_id\":2");
        json.Should().Contain("\"type\":\"message_callback\"");
        json.Should().NotContain("\"message\"");
        json.Should().NotContain("\"callback_query\"");
    }
}


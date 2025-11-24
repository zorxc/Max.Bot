using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class ChatTests
{
    [Fact]
    public void Deserialize_ShouldDeserializeChat()
    {
        // Arrange - using official API field names: chat_id and type values (dialog, chat, channel)
        var json = """{"chat_id":123,"type":"dialog","title":"Test Chat","username":"testchat","first_name":"Test","last_name":"Chat"}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Chat>(json);

        // Assert
        result.Should().NotBeNull();
        result!.ChatId.Should().Be(123);
        result.Id.Should().Be(123); // Backward compatibility alias
        result.Type.Should().Be(ChatType.Dialog);
        result.Title.Should().Be("Test Chat");
        result.Username.Should().Be("testchat");
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("Chat");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeChatWithDifferentTypes()
    {
        // Arrange - using official API type values: dialog, chat, channel
        var testCases = new[]
        {
            ("""{"chat_id":123,"type":"dialog"}""", ChatType.Dialog),
            ("""{"chat_id":456,"type":"chat","title":"Group Chat"}""", ChatType.Chat),
            ("""{"chat_id":789,"type":"channel","title":"Channel Chat"}""", ChatType.Channel)
        };

        // Act & Assert
        foreach (var (json, expectedType) in testCases)
        {
            var result = MaxJsonSerializer.Deserialize<Chat>(json);
            result.Should().NotBeNull();
            result!.Type.Should().Be(expectedType);
        }
    }

    [Fact]
    public void Serialize_ShouldSerializeChat()
    {
        // Arrange
        var chat = new Chat
        {
            ChatId = 123,
            Type = ChatType.Dialog,
            Title = "Test Chat",
            Username = "testchat"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(chat);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"chat_id\":123");
        json.Should().Contain("\"type\":\"dialog\"");
        json.Should().Contain("\"title\":\"Test Chat\"");
        json.Should().Contain("\"username\":\"testchat\"");
    }

    [Fact]
    public void Serialize_ShouldNotIncludeNullFields()
    {
        // Arrange
        var chat = new Chat
        {
            ChatId = 123,
            Type = ChatType.Dialog
        };

        // Act
        var json = MaxJsonSerializer.Serialize(chat);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"chat_id\":123");
        json.Should().Contain("\"type\":\"dialog\"");
        json.Should().NotContain("\"title\"");
        json.Should().NotContain("\"username\"");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeChatWithNewFields()
    {
        // Arrange - using official API field names
        var json = """{"chat_id":123,"type":"chat","status":"active","title":"Test Chat","last_event_time":1609459200,"participants_count":5,"owner_id":789,"is_public":true,"link":"https://max.ru/chat/123","description":"Test description"}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Chat>(json);

        // Assert
        result.Should().NotBeNull();
        result!.ChatId.Should().Be(123);
        result.Id.Should().Be(123); // Backward compatibility alias
        result.Type.Should().Be(ChatType.Chat);
        result.Status.Should().Be(ChatStatus.Active);
        result.Title.Should().Be("Test Chat");
        result.LastEventTime.Should().Be(1609459200);
        result.ParticipantsCount.Should().Be(5);
        result.OwnerId.Should().Be(789);
        result.IsPublic.Should().BeTrue();
        result.Link.Should().Be("https://max.ru/chat/123");
        result.Description.Should().Be("Test description");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeChatWithAllStatuses()
    {
        // Arrange
        var testCases = new[]
        {
            ("""{"chat_id":123,"status":"active"}""", ChatStatus.Active),
            ("""{"chat_id":123,"status":"removed"}""", ChatStatus.Removed),
            ("""{"chat_id":123,"status":"left"}""", ChatStatus.Left),
            ("""{"chat_id":123,"status":"closed"}""", ChatStatus.Closed)
        };

        // Act & Assert
        foreach (var (json, expectedStatus) in testCases)
        {
            var result = MaxJsonSerializer.Deserialize<Chat>(json);
            result.Should().NotBeNull();
            result!.Status.Should().Be(expectedStatus);
        }
    }

    [Fact]
    public void Serialize_ShouldSerializeChatWithNewFields()
    {
        // Arrange
        var chat = new Chat
        {
            ChatId = 123,
            Type = ChatType.Chat,
            Status = ChatStatus.Active,
            Title = "Test Chat",
            LastEventTime = 1609459200,
            ParticipantsCount = 5,
            OwnerId = 789,
            IsPublic = true,
            Link = "https://max.ru/chat/123",
            Description = "Test description"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(chat);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"chat_id\":123");
        json.Should().Contain("\"type\":\"chat\"");
        json.Should().Contain("\"status\":\"active\"");
        json.Should().Contain("\"title\":\"Test Chat\"");
        json.Should().Contain("\"last_event_time\":1609459200");
        json.Should().Contain("\"participants_count\":5");
        json.Should().Contain("\"owner_id\":789");
        json.Should().Contain("\"is_public\":true");
        json.Should().Contain("\"link\":\"https://max.ru/chat/123\"");
        json.Should().Contain("\"description\":\"Test description\"");
    }
}

public class ChatStatusTests
{
    [Theory]
    [InlineData("active", ChatStatus.Active)]
    [InlineData("removed", ChatStatus.Removed)]
    [InlineData("left", ChatStatus.Left)]
    [InlineData("closed", ChatStatus.Closed)]
    public void Deserialize_ShouldParseJsonString(string jsonValue, ChatStatus expected)
    {
        // Arrange
        var json = $"{{\"status\":\"{jsonValue}\"}}";

        // Act
        var result = MaxJsonSerializer.Deserialize<Chat>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(expected);
    }

    [Theory]
    [InlineData(ChatStatus.Active, "active")]
    [InlineData(ChatStatus.Removed, "removed")]
    [InlineData(ChatStatus.Left, "left")]
    [InlineData(ChatStatus.Closed, "closed")]
    public void Serialize_ShouldConvertToJsonString(ChatStatus value, string expectedJsonValue)
    {
        // Arrange
        var chat = new Chat { Status = value };

        // Act
        var json = MaxJsonSerializer.Serialize(chat);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain($"\"status\":\"{expectedJsonValue}\"");
    }
}


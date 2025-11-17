using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class MessageTests
{
    [Fact]
    public void Deserialize_ShouldDeserializeMessage()
    {
        // Arrange
        var json = """{"id":123,"chat":{"id":456,"type":"private"},"from":{"user_id":789,"username":"testuser","is_bot":false},"text":"Hello","date":1609459200,"type":"text"}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Message>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Chat.Should().NotBeNull();
        result.Chat!.Id.Should().Be(456);
        result.Chat.Type.Should().Be(ChatType.Private);
        result.From.Should().NotBeNull();
        result.From!.Id.Should().Be(789);
        result.From.Username.Should().Be("testuser");
        result.Text.Should().Be("Hello");
        result.Date.Should().BeCloseTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromSeconds(1));
        result.Type.Should().Be(MessageType.Text);
    }

    [Fact]
    public void Deserialize_ShouldDeserializeMessageWithNullFields()
    {
        // Arrange
        var json = """{"id":123,"date":1609459200}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Message>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Chat.Should().BeNull();
        result.From.Should().BeNull();
        result.Text.Should().BeNull();
        result.Type.Should().BeNull();
    }

    [Fact]
    public void Serialize_ShouldSerializeMessage()
    {
        // Arrange
        var message = new Message
        {
            Id = 123,
            Chat = new Chat { Id = 456, Type = ChatType.Private },
            From = new User { Id = 789, Username = "testuser" },
            Text = "Hello",
            Date = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Type = MessageType.Text
        };

        // Act
        var json = MaxJsonSerializer.Serialize(message);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"chat\"");
        json.Should().Contain("\"from\"");
        json.Should().Contain("\"text\":\"Hello\"");
        json.Should().Contain("\"date\":1609459200");
        json.Should().Contain("\"type\":\"text\"");
    }

    [Fact]
    public void Serialize_ShouldNotIncludeNullFields()
    {
        // Arrange
        var message = new Message
        {
            Id = 123,
            Date = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var json = MaxJsonSerializer.Serialize(message);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"date\":1609459200");
        json.Should().NotContain("\"chat\"");
        json.Should().NotContain("\"from\"");
        json.Should().NotContain("\"text\"");
        json.Should().NotContain("\"type\"");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeMessageWithNewFields()
    {
        // Arrange
        var json = """{"id":123,"sender":{"user_id":789,"username":"testuser"},"recipient":{"id":456,"type":"private"},"timestamp":1609459200,"body":{"text":"Hello","attachments":[]},"stat":{"read_count":5},"url":"https://max.ru/message/123"}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Message>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Sender.Should().NotBeNull();
        result.Sender!.Id.Should().Be(789);
        result.Timestamp.Should().Be(1609459200);
        result.Body.Should().NotBeNull();
        result.Body!.Text.Should().Be("Hello");
        result.Stat.Should().NotBeNull();
        result.Stat!.ReadCount.Should().Be(5);
        result.Url.Should().Be("https://max.ru/message/123");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeMessageWithLink()
    {
        // Arrange
        var json = """{"id":123,"timestamp":1609459200,"link":{"id":456,"timestamp":1609362800,"text":"Original message"}}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Message>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Link.Should().NotBeNull();
        result.Link!.Id.Should().Be(456);
        result.Link.Text.Should().Be("Original message");
    }

    [Fact]
    public void Serialize_ShouldSerializeMessageWithNewFields()
    {
        // Arrange
        var message = new Message
        {
            Id = 123,
            Sender = new User { Id = 789, Username = "testuser" },
            Timestamp = 1609459200,
            Body = new MessageBody { Text = "Hello", Attachments = Array.Empty<Attachment>() },
            Stat = new MessageStat { ReadCount = 5 },
            Url = "https://max.ru/message/123"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(message);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"sender\"");
        json.Should().Contain("\"timestamp\":1609459200");
        json.Should().Contain("\"body\"");
        json.Should().Contain("\"stat\"");
        json.Should().Contain("\"url\":\"https://max.ru/message/123\"");
    }

    [Fact]
    public void MessageBody_ShouldDeserialize()
    {
        // Arrange
        var json = """{"text":"Hello","attachments":[{"type":"image","photo":{"id":1,"file_id":"photo1","width":100,"height":100}}]}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<MessageBody>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Text.Should().Be("Hello");
        result.Attachments.Should().NotBeNull();
        result.Attachments!.Length.Should().Be(1);
    }

    [Fact]
    public void MessageStat_ShouldDeserialize()
    {
        // Arrange
        var json = """{"read_count":10}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<MessageStat>(json);

        // Assert
        result.Should().NotBeNull();
        result!.ReadCount.Should().Be(10);
    }
}


using System.Net;
using System.Net.Http;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;
using Moq;
using Xunit;

namespace Max.Bot.Tests.Unit.Api;

public class MessagesApiTests
{
    private readonly Mock<IMaxHttpClient> _mockHttpClient;
    private readonly MaxBotOptions _options;

    public MessagesApiTests()
    {
        _mockHttpClient = new Mock<IMaxHttpClient>();
        _options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnMessage_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var text = "Hello, World!";
        var expectedMessage = new Message
        {
            Id = 789,
            Text = text,
            Chat = new Chat { Id = chatId }
        };

        var response = new MessageResponse
        {
            Message = expectedMessage
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<MessageResponse>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/messages" &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.SendMessageAsync(chatId, text);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMessage.Id);
        result.Text.Should().Be(expectedMessage.Text);
        result.Chat.Should().NotBeNull();
        result.Chat!.Id.Should().Be(chatId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SendMessageAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.SendMessageAsync(chatId, "test");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendMessageAsync_ShouldThrowArgumentException_WhenTextIsNullOrEmpty(string? text)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.SendMessageAsync(123456L, text!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("text");
    }

    [Fact]
    public async Task GetMessagesAsync_ShouldReturnMessages_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedMessages = new[]
        {
            new Message { Id = 1, Text = "Message 1", Chat = new Chat { Id = chatId } },
            new Message { Id = 2, Text = "Message 2", Chat = new Chat { Id = chatId } }
        };

        var response = new Response<Message[]>
        {
            Ok = true,
            Result = expectedMessages
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("chat_id") &&
                    req.QueryParameters["chat_id"] == chatId.ToString()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.GetMessagesAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetMessagesAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.GetMessagesAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    [Fact]
    public async Task EditMessageAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var messageId = "msg-123";
        var request = new EditMessageRequest
        {
            Text = "Updated text",
            Format = TextFormat.Markdown
        };

        var response = new Response
        {
            Success = true,
            Message = "Message edited successfully"
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Put &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("message_id") &&
                    req.QueryParameters["message_id"] == messageId &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.EditMessageAsync(messageId, request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Message edited successfully");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EditMessageAsync_ShouldThrowArgumentException_WhenMessageIdIsNullOrEmpty(string? messageId)
    {
        // Arrange
        var request = new EditMessageRequest { Text = "Updated text" };
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.EditMessageAsync(messageId!, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }

    [Fact]
    public async Task EditMessageAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.EditMessageAsync("msg-123", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var messageId = "msg-123";
        var response = new Response
        {
            Success = true,
            Message = "Message deleted successfully"
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("message_id") &&
                    req.QueryParameters["message_id"] == messageId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.DeleteMessageAsync(messageId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Message deleted successfully");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteMessageAsync_ShouldThrowArgumentException_WhenMessageIdIsNullOrEmpty(string? messageId)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.DeleteMessageAsync(messageId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }

    [Fact]
    public async Task GetMessageAsync_ShouldReturnMessage_WhenRequestSucceeds()
    {
        // Arrange
        var messageId = "msg-123";
        var expectedMessage = new Message
        {
            Id = 123,
            Text = "Test message",
            Chat = new Chat { Id = 456 }
        };

        var response = new Response<Message>
        {
            Ok = true,
            Result = expectedMessage
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/messages/{messageId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.GetMessageAsync(messageId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMessage.Id);
        result.Text.Should().Be(expectedMessage.Text);
        result.Chat.Should().NotBeNull();
        result.Chat!.Id.Should().Be(456);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetMessageAsync_ShouldThrowArgumentException_WhenMessageIdIsNullOrEmpty(string? messageId)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.GetMessageAsync(messageId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }

    [Fact]
    public async Task GetVideoAsync_ShouldReturnVideo_WhenRequestSucceeds()
    {
        // Arrange
        var videoToken = "video-token-123";
        var expectedVideo = new Video
        {
            Id = 789,
            FileId = "file-123",
            Width = 1920,
            Height = 1080,
            Duration = 60
        };

        var response = new Response<Video>
        {
            Ok = true,
            Result = expectedVideo
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/videos/{videoToken}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.GetVideoAsync(videoToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedVideo.Id);
        result.FileId.Should().Be(expectedVideo.FileId);
        result.Width.Should().Be(expectedVideo.Width);
        result.Height.Should().Be(expectedVideo.Height);
        result.Duration.Should().Be(expectedVideo.Duration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetVideoAsync_ShouldThrowArgumentException_WhenVideoTokenIsNullOrEmpty(string? videoToken)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.GetVideoAsync(videoToken!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("videoToken");
    }

    [Fact]
    public async Task AnswerCallbackQueryAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var callbackQueryId = "callback-123";
        var request = new AnswerCallbackQueryRequest
        {
            Notification = "Button clicked!",
            Message = new NewMessageBody
            {
                Text = "Updated message text"
            }
        };

        var response = new Response
        {
            Success = true,
            Message = "Callback query answered successfully"
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/answers" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("callback_query_id") &&
                    req.QueryParameters["callback_query_id"] == callbackQueryId &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.AnswerCallbackQueryAsync(callbackQueryId, request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Callback query answered successfully");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnswerCallbackQueryAsync_ShouldThrowArgumentException_WhenCallbackQueryIdIsNullOrEmpty(string? callbackQueryId)
    {
        // Arrange
        var request = new AnswerCallbackQueryRequest { Notification = "Test" };
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.AnswerCallbackQueryAsync(callbackQueryId!, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("callbackQueryId");
    }

    [Fact]
    public async Task AnswerCallbackQueryAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.AnswerCallbackQueryAsync("callback-123", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task SendMessageAsync_WithSendMessageRequest_ShouldReturnMessage_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var request = new SendMessageRequest
        {
            Text = "Hello, World!",
            Notify = true,
            Format = TextFormat.Markdown
        };

        var expectedMessage = new Message
        {
            Id = 789,
            Text = request.Text,
            Chat = new Chat { Id = chatId }
        };

        var response = new MessageResponse
        {
            Message = expectedMessage
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<MessageResponse>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("chat_id") &&
                    req.QueryParameters["chat_id"] == chatId.ToString() &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.SendMessageAsync(request, chatId: chatId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMessage.Id);
        result.Text.Should().Be(expectedMessage.Text);
    }

    [Fact]
    public async Task SendMessageAsync_WithSendMessageRequest_ShouldThrowArgumentException_WhenBothChatIdAndUserIdProvided()
    {
        // Arrange
        var request = new SendMessageRequest { Text = "Test" };
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.SendMessageAsync(request, chatId: 1, userId: 2);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*chatId*userId*");
    }

    [Fact]
    public async Task SendMessageAsync_WithSendMessageRequest_ShouldThrowArgumentException_WhenNeitherChatIdNorUserIdProvided()
    {
        // Arrange
        var request = new SendMessageRequest { Text = "Test" };
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.SendMessageAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*chatId*userId*");
    }

    [Fact]
    public async Task SendMessageAsync_WithSendMessageRequest_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.SendMessageAsync(null!, chatId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task SendMessageWithAttachmentAsync_ShouldReturnMessage_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var attachment = new AttachmentRequest
        {
            Type = "image",
            Payload = new { token = "test-token" }
        };

        var expectedMessage = new Message
        {
            Id = 789,
            Text = "With attachment",
            Chat = new Chat { Id = chatId }
        };

        var response = new MessageResponse
        {
            Message = expectedMessage
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<MessageResponse>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("chat_id") &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.SendMessageWithAttachmentAsync(attachment, chatId: chatId, text: "With attachment");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMessage.Id);
    }

    [Fact]
    public async Task SendMessageWithAttachmentAsync_ShouldThrowArgumentNullException_WhenAttachmentIsNull()
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.SendMessageWithAttachmentAsync(null!, chatId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("attachment");
    }

    [Fact]
    public async Task ForwardMessageAsync_ShouldReturnMessage_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var messageId = 789L;

        var expectedMessage = new Message
        {
            Id = 999,
            Text = "Forwarded",
            Chat = new Chat { Id = chatId }
        };

        var response = new MessageResponse
        {
            Message = expectedMessage
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<MessageResponse>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("chat_id") &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.ForwardMessageAsync(messageId, chatId: chatId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMessage.Id);
    }

    [Fact]
    public async Task ForwardMessageAsync_ShouldThrowArgumentException_WhenMessageIdIsZero()
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.ForwardMessageAsync(0, chatId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }

    [Fact]
    public async Task ReplyToMessageAsync_ShouldReturnMessage_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var messageId = 789L;
        var text = "Reply text";

        var expectedMessage = new Message
        {
            Id = 999,
            Text = text,
            Chat = new Chat { Id = chatId }
        };

        var response = new MessageResponse
        {
            Message = expectedMessage
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<MessageResponse>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == "/messages" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("chat_id") &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var result = await messagesApi.ReplyToMessageAsync(messageId, text, chatId: chatId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMessage.Id);
        result.Text.Should().Be(text);
    }

    [Fact]
    public async Task ReplyToMessageAsync_ShouldThrowArgumentException_WhenMessageIdIsZero()
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.ReplyToMessageAsync(0, "text", chatId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("messageId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReplyToMessageAsync_ShouldThrowArgumentException_WhenTextIsNullOrEmpty(string? text)
    {
        // Arrange
        var messagesApi = new MessagesApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await messagesApi.ReplyToMessageAsync(1, text!, chatId: 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("text");
    }
}


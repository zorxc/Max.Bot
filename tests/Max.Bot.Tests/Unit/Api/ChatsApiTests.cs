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

public class ChatsApiTests
{
    private readonly Mock<IMaxHttpClient> _mockHttpClient;
    private readonly MaxBotOptions _options;

    public ChatsApiTests()
    {
        _mockHttpClient = new Mock<IMaxHttpClient>();
        _options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };
    }

    [Fact]
    public async Task GetChatAsync_ShouldReturnChat_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedChat = new Chat
        {
            Id = chatId,
            Type = ChatType.Private,
            Username = "test_user"
        };

        var response = new Response<Chat>
        {
            Ok = true,
            Result = expectedChat
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedChat.Id);
        result.Type.Should().Be(expectedChat.Type);
        result.Username.Should().Be(expectedChat.Username);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetChatAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    [Fact]
    public async Task GetChatsAsync_ShouldReturnChats_WhenRequestSucceeds()
    {
        // Arrange
        var expectedChats = new[]
        {
            new Chat { Id = 1, Type = ChatType.Private },
            new Chat { Id = 2, Type = ChatType.Group }
        };

        var response = new Response<Chat[]>
        {
            Ok = true,
            Result = expectedChats
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == "/chats"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
    }

    #region GetChatByLinkAsync Tests

    [Fact]
    public async Task GetChatByLinkAsync_ShouldReturnChat_WhenRequestSucceeds()
    {
        // Arrange
        var chatLink = "@test_chat";
        var expectedChat = new Chat
        {
            Id = 123456L,
            Type = ChatType.Group,
            Title = "Test Chat"
        };

        var response = new Response<Chat>
        {
            Ok = true,
            Result = expectedChat
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/%40test_chat"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatByLinkAsync(chatLink);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedChat.Id);
        result.Title.Should().Be(expectedChat.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetChatByLinkAsync_ShouldThrowArgumentException_WhenChatLinkIsInvalid(string? chatLink)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatByLinkAsync(chatLink!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatLink");
    }

    [Fact]
    public async Task GetChatByLinkAsync_ShouldThrowMaxApiException_WhenApiReturnsError()
    {
        // Arrange
        var chatLink = "@invalid_chat";
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/%40invalid_chat"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(""); // Empty response body to trigger error

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatByLinkAsync(chatLink);

        // Assert
        await act.Should().ThrowAsync<MaxApiException>()
            .WithMessage("*empty response body*");
    }

    #endregion

    #region UpdateChatAsync Tests

    [Fact]
    public async Task UpdateChatAsync_ShouldReturnUpdatedChat_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var request = new UpdateChatRequest
        {
            Title = "Updated Title",
            Notify = true
        };

        var expectedChat = new Chat
        {
            Id = chatId,
            Type = ChatType.Group,
            Title = "Updated Title"
        };

        var response = new Response<Chat>
        {
            Ok = true,
            Result = expectedChat
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response<Chat>>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Patch &&
                    req.Endpoint == $"/chats/{chatId}" &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.UpdateChatAsync(chatId, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedChat.Id);
        result.Title.Should().Be(expectedChat.Title);
    }

    [Fact]
    public async Task UpdateChatAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.UpdateChatAsync(123456L, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateChatAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var request = new UpdateChatRequest { Title = "Test" };
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.UpdateChatAsync(chatId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region DeleteChatAsync Tests

    [Fact]
    public async Task DeleteChatAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedResponse = new Response { Success = true, Message = "Chat deleted" };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.Endpoint == $"/chats/{chatId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.DeleteChatAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Chat deleted");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteChatAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.DeleteChatAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region SendChatActionAsync Tests

    [Fact]
    public async Task SendChatActionAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var request = new SendChatActionRequest { Action = ChatAction.TypingOn };
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == $"/chats/{chatId}/actions" &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.SendChatActionAsync(chatId, request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendChatActionAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.SendChatActionAsync(123456L, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SendChatActionAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var request = new SendChatActionRequest { Action = ChatAction.TypingOn };
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.SendChatActionAsync(chatId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region GetPinnedMessageAsync Tests

    [Fact]
    public async Task GetPinnedMessageAsync_ShouldReturnMessage_WhenMessageIsPinned()
    {
        // Arrange
        var chatId = 123456L;
        var expectedMessage = new Message
        {
            Id = 789L,
            Text = "Pinned message",
            Type = MessageType.Text
        };

        var response = new Response<Message>
        {
            Ok = true,
            Result = expectedMessage
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response<Message>>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}/pin"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetPinnedMessageAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedMessage.Id);
        result.Text.Should().Be(expectedMessage.Text);
    }

    [Fact]
    public async Task GetPinnedMessageAsync_ShouldReturnNull_WhenNoMessageIsPinned()
    {
        // Arrange
        var chatId = 123456L;

        var response = new Response<Message>
        {
            Ok = true,
            Result = null
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response<Message>>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}/pin"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetPinnedMessageAsync(chatId);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPinnedMessageAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetPinnedMessageAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region PinMessageAsync Tests

    [Fact]
    public async Task PinMessageAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var request = new PinMessageRequest { MessageId = "789", Notify = true };
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Put &&
                    req.Endpoint == $"/chats/{chatId}/pin" &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.PinMessageAsync(chatId, request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PinMessageAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.PinMessageAsync(123456L, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PinMessageAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var request = new PinMessageRequest { MessageId = "789" };
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.PinMessageAsync(chatId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region UnpinMessageAsync Tests

    [Fact]
    public async Task UnpinMessageAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.Endpoint == $"/chats/{chatId}/pin"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.UnpinMessageAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UnpinMessageAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.UnpinMessageAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region GetChatMembershipAsync Tests

    [Fact]
    public async Task GetChatMembershipAsync_ShouldReturnChat_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedChat = new Chat
        {
            Id = chatId,
            Type = ChatType.Group,
            Title = "Test Chat"
        };

        var response = new Response<Chat>
        {
            Ok = true,
            Result = expectedChat
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}/members/me"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatMembershipAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedChat.Id);
        result.Title.Should().Be(expectedChat.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetChatMembershipAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatMembershipAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region LeaveChatAsync Tests

    [Fact]
    public async Task LeaveChatAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedResponse = new Response { Success = true, Message = "Left chat" };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.Endpoint == $"/chats/{chatId}/members/me"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.LeaveChatAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Left chat");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task LeaveChatAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.LeaveChatAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region GetChatAdminsAsync Tests

    [Fact]
    public async Task GetChatAdminsAsync_ShouldReturnAdmins_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedAdmins = new[]
        {
            new User { Id = 100L, FirstName = "Admin1" },
            new User { Id = 200L, FirstName = "Admin2" }
        };

        var response = new Response<User[]>
        {
            Ok = true,
            Result = expectedAdmins
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}/members/admins"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatAdminsAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(100L);
        result[1].Id.Should().Be(200L);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetChatAdminsAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatAdminsAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region AddChatAdminAsync Tests

    [Fact]
    public async Task AddChatAdminAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var request = new AddChatAdminRequest { UserId = 789L };
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == $"/chats/{chatId}/members/admins" &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.AddChatAdminAsync(chatId, request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddChatAdminAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.AddChatAdminAsync(123456L, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddChatAdminAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var request = new AddChatAdminRequest { UserId = 789L };
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.AddChatAdminAsync(chatId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region RemoveChatAdminAsync Tests

    [Fact]
    public async Task RemoveChatAdminAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var userId = 789L;
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.Endpoint == $"/chats/{chatId}/members/admins/{userId}"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.RemoveChatAdminAsync(chatId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RemoveChatAdminAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.RemoveChatAdminAsync(chatId, 789L);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RemoveChatAdminAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(long userId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.RemoveChatAdminAsync(123456L, userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("userId");
    }

    #endregion

    #region GetChatMembersAsync Tests

    [Fact]
    public async Task GetChatMembersAsync_ShouldReturnMembers_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var expectedMembers = new[]
        {
            new User { Id = 100L, FirstName = "User1" },
            new User { Id = 200L, FirstName = "User2" }
        };

        var response = new Response<User[]>
        {
            Ok = true,
            Result = expectedMembers
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}/members" &&
                    req.QueryParameters == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatMembersAsync(chatId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(100L);
        result[1].Id.Should().Be(200L);
    }

    [Fact]
    public async Task GetChatMembersAsync_ShouldReturnMembers_WhenOffsetAndLimitProvided()
    {
        // Arrange
        var chatId = 123456L;
        var offset = 10;
        var limit = 20;
        var expectedMembers = new[]
        {
            new User { Id = 100L, FirstName = "User1" }
        };

        var response = new Response<User[]>
        {
            Ok = true,
            Result = expectedMembers
        };

        var responseJson = MaxJsonSerializer.Serialize(response);
        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Endpoint == $"/chats/{chatId}/members" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("offset") &&
                    req.QueryParameters["offset"] == "10" &&
                    req.QueryParameters.ContainsKey("limit") &&
                    req.QueryParameters["limit"] == "20"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatMembersAsync(chatId, offset, limit);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetChatMembersAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatMembersAsync(chatId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region AddChatMembersAsync Tests

    [Fact]
    public async Task AddChatMembersAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var request = new AddChatMembersRequest { UserIds = new[] { 100L, 200L } };
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Post &&
                    req.Endpoint == $"/chats/{chatId}/members" &&
                    req.Body != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.AddChatMembersAsync(chatId, request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddChatMembersAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.AddChatMembersAsync(123456L, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddChatMembersAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var request = new AddChatMembersRequest { UserIds = new[] { 100L } };
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.AddChatMembersAsync(chatId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    #endregion

    #region RemoveChatMemberAsync Tests

    [Fact]
    public async Task RemoveChatMemberAsync_ShouldReturnResponse_WhenRequestSucceeds()
    {
        // Arrange
        var chatId = 123456L;
        var userId = 789L;
        var expectedResponse = new Response { Success = true };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.Is<MaxApiRequest>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.Endpoint == $"/chats/{chatId}/members" &&
                    req.QueryParameters != null &&
                    req.QueryParameters.ContainsKey("user_id") &&
                    req.QueryParameters["user_id"] == "789"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.RemoveChatMemberAsync(chatId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RemoveChatMemberAsync_ShouldThrowArgumentException_WhenChatIdIsInvalid(long chatId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.RemoveChatMemberAsync(chatId, 789L);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("chatId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RemoveChatMemberAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(long userId)
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.RemoveChatMemberAsync(123456L, userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("userId");
    }

    #endregion
}


using System.Net.Http;
using System.Reflection;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Moq;
using Xunit;

namespace Max.Bot.Tests.Unit.Api;

public class BaseApiTests
{
    private readonly Mock<IMaxHttpClient> _mockHttpClient;
    private readonly MaxBotOptions _options;

    public BaseApiTests()
    {
        _mockHttpClient = new Mock<IMaxHttpClient>();
        _options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldReturnResponse_WhenTIsResponse()
    {
        // Arrange - Test the special case where T is Response (not Response<T>) - line 99-110
        var expectedResponse = new Response
        {
            Success = true
        };

        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.IsAny<MaxApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act - DeleteChatAsync returns Response, not Response<T>
        var result = await chatsApi.DeleteChatAsync(123456L);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldThrowMaxApiException_WhenResponseIsNull()
    {
        // Arrange - Test the case where Response is null (line 102-108)
        Response? nullResponse = null;
        _mockHttpClient
            .Setup(x => x.SendAsync<Response>(
                It.IsAny<MaxApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(nullResponse!);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.DeleteChatAsync(123456L);

        // Assert
        await act.Should().ThrowAsync<MaxApiException>()
            .WithMessage("*null response*");
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldDeserializeDirectly_WhenResponseTFormatDoesNotMatch()
    {
        // Arrange - Test GET request where Response<T> format doesn't match, but direct T does (line 137-140, 143-149)
        var expectedChat = new Chat
        {
            Id = 123456L,
            Type = ChatType.Private
        };

        // Return direct Chat JSON, not wrapped in Response<Chat>
        var directJson = MaxJsonSerializer.Serialize(expectedChat);

        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.IsAny<MaxApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(directJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var result = await chatsApi.GetChatAsync(123456L);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedChat.Id);
        result.Type.Should().Be(expectedChat.Type);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldThrowMaxApiException_WhenBothDeserializationsFail()
    {
        // Arrange - Test GET request where both Response<T> and direct T deserialization fail (line 151-162)
        var invalidJson = "{ invalid json }";

        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.IsAny<MaxApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatAsync(123456L);

        // Assert
        await act.Should().ThrowAsync<MaxApiException>()
            .WithMessage("*could not be deserialized*");
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldThrowMaxApiException_WhenDirectDeserializationReturnsNull()
    {
        // Arrange - Test GET request where direct deserialization returns null (line 146-149, 159-162)
        // Return valid JSON that deserializes to null
        var nullJson = "null";

        _mockHttpClient
            .Setup(x => x.SendAsyncRaw(
                It.IsAny<MaxApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(nullJson);

        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act
        var act = async () => await chatsApi.GetChatAsync(123456L);

        // Assert
        await act.Should().ThrowAsync<MaxApiException>()
            .WithMessage("*could not be deserialized*");
    }

    [Fact]
    public void CreateRequest_ShouldAddSlash_WhenPathDoesNotStartWithSlash()
    {
        // Arrange - Test BuildEndpoint when path doesn't start with '/' (line 52)
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act - Use reflection to call protected CreateRequest method
        var method = typeof(ChatsApi).GetMethod("CreateRequest", BindingFlags.NonPublic | BindingFlags.Instance);
        var request = (MaxApiRequest)method!.Invoke(chatsApi, new object?[] { HttpMethod.Get, "test-endpoint", null, null })!;

        // Assert
        request.Endpoint.Should().Be("/test-endpoint");
    }

    [Fact]
    public void CreateRequest_ShouldThrowArgumentException_WhenPathIsEmpty()
    {
        // Arrange - Test BuildEndpoint with empty path (line 45-48)
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act - Use reflection to call protected CreateRequest method
        var method = typeof(ChatsApi).GetMethod("CreateRequest", BindingFlags.NonPublic | BindingFlags.Instance);
        var act = () => method!.Invoke(chatsApi, new object?[] { HttpMethod.Get, "", null, null });

        // Assert
        act.Should().Throw<TargetInvocationException>()
            .WithInnerException<ArgumentException>()
            .WithParameterName("path");
    }

    [Fact]
    public void CreateRequest_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var chatsApi = new ChatsApi(_mockHttpClient.Object, _options);

        // Act - Use reflection to call protected CreateRequest method
        var method = typeof(ChatsApi).GetMethod("CreateRequest", BindingFlags.NonPublic | BindingFlags.Instance);
        var request = (MaxApiRequest)method!.Invoke(chatsApi, new object?[] { HttpMethod.Get, "/test", null, null })!;

        // Assert
        request.Headers.Should().NotBeNull();
        request.Headers.Should().ContainKey("Authorization");
        request.Headers!["Authorization"].Should().Be("test-token-123");
    }
}


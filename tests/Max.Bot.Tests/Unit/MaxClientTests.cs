using FluentAssertions;
using Max.Bot;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Xunit;

namespace Max.Bot.Tests.Unit;

public class MaxClientTests
{
    [Fact]
    public void MaxClient_ShouldInitialize_WithToken()
    {
        // Act
        var client = new MaxClient("test-token-123");

        // Assert
        client.Should().NotBeNull();
        client.Bot.Should().NotBeNull().And.BeAssignableTo<IBotApi>();
        client.Messages.Should().NotBeNull().And.BeAssignableTo<IMessagesApi>();
        client.Chats.Should().NotBeNull().And.BeAssignableTo<IChatsApi>();
        client.Users.Should().NotBeNull().And.BeAssignableTo<IUsersApi>();
    }

    [Fact]
    public void MaxClient_ShouldInitialize_WithOptions()
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };

        // Act
        var client = new MaxClient(options);

        // Assert
        client.Should().NotBeNull();
        client.Bot.Should().NotBeNull();
        client.Messages.Should().NotBeNull();
        client.Chats.Should().NotBeNull();
        client.Users.Should().NotBeNull();
    }

    [Fact]
    public void MaxClient_ShouldInitialize_WithOptionsAndHttpClient()
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = "test-token-123",
            BaseUrl = "https://api.max.ru/bot"
        };
        var httpClient = new System.Net.Http.HttpClient();

        // Act
        var client = new MaxClient(options, httpClient);

        // Assert
        client.Should().NotBeNull();
        client.Bot.Should().NotBeNull();
        client.Messages.Should().NotBeNull();
        client.Chats.Should().NotBeNull();
        client.Users.Should().NotBeNull();
    }

    [Fact]
    public void MaxClient_ShouldThrowArgumentException_WhenTokenIsEmpty()
    {
        // Act
        var act = () => new MaxClient(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MaxClient_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Act
        var act = () => new MaxClient((MaxBotOptions)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MaxClient_ShouldThrowArgumentException_WhenTokenIsInvalid(string? token)
    {
        // Arrange
        var options = new MaxBotOptions
        {
            Token = token!
        };

        // Act
        var act = () => new MaxClient(options);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}


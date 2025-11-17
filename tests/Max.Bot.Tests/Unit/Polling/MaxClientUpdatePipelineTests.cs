using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Max.Bot.Configuration;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Moq;
using Xunit;

namespace Max.Bot.Tests.Unit.Polling;

public class MaxClientUpdatePipelineTests
{
    [Fact]
    public async Task ProcessWebhookAsync_ShouldInvokeHandler_WhenUserAllowed()
    {
        // Arrange
        var options = CreateOptions();
        options.Handling.AllowedUsernames.Add("tester");
        var client = CreateClient(options);

        var handlerMock = new Mock<IUpdateHandler>();
        handlerMock
            .Setup(h => h.HandleUpdateAsync(It.IsAny<UpdateContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handlerMock
            .Setup(h => h.HandleMessageAsync(It.IsAny<UpdateContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handlerMock
            .Setup(h => h.HandleUnknownUpdateAsync(It.IsAny<UpdateContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handlerMock
            .Setup(h => h.HandleCallbackQueryAsync(It.IsAny<UpdateContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var update = new Update
        {
            UpdateId = 1,
            UpdateTypeRaw = "message_created",
            Message = new Message { From = new User { Username = "tester" } }
        };

        // Act
        await client.ProcessWebhookAsync(update, handlerMock.Object, null, CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.HandleMessageAsync(It.IsAny<UpdateContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldSkipHandler_WhenUserNotAllowed()
    {
        // Arrange
        var options = CreateOptions();
        options.Handling.AllowedUsernames.Add("allowed");
        var client = CreateClient(options);
        var handlerMock = new Mock<IUpdateHandler>(MockBehavior.Strict);

        var update = new Update
        {
            UpdateId = 2,
            UpdateTypeRaw = "message_created",
            Message = new Message { From = new User { Username = "ignored" } }
        };

        // Act
        await client.ProcessWebhookAsync(update, handlerMock.Object, null, CancellationToken.None);

        // Assert
        handlerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConfigureWebhookAsync_ShouldThrow_WhenUrlIsNotHttpsAndEnforced()
    {
        // Arrange
        var options = CreateOptions();
        options.Webhook.EnforceHttps = true;
        var client = CreateClient(options);

        // Act
        var act = async () => await client.ConfigureWebhookAsync("http://example.com/hook");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static MaxClient CreateClient(MaxBotOptions options)
    {
        var httpClient = new HttpClient(new HttpClientHandler());
        return new MaxClient(options, httpClient);
    }

    private static MaxBotOptions CreateOptions()
    {
        return new MaxBotOptions
        {
            Token = "token",
            BaseUrl = "https://api.max.ru/bot"
        };
    }
}





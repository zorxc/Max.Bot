using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Moq;
using Xunit;

namespace Max.Bot.Tests.Unit.Polling;

public class DelegatingUpdateHandlerTests
{
    [Fact]
    public async Task DelegatingHandler_ShouldInvokeMessageDelegate()
    {
        // Arrange
        var invoked = false;
        var handler = new DelegatingUpdateHandler(
            onMessage: (_, _) =>
            {
                invoked = true;
                return Task.CompletedTask;
            });

        var context = CreateContext(UpdateType.Message);

        // Act
        await handler.HandleMessageAsync(context, CancellationToken.None);

        // Assert
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task DelegatingHandler_ShouldSkipNullDelegates()
    {
        // Arrange
        var handler = new DelegatingUpdateHandler();
        var context = CreateContext(UpdateType.CallbackQuery);

        // Act / Assert (no exception)
        await handler.HandleCallbackQueryAsync(context, CancellationToken.None);
    }

    private static UpdateContext CreateContext(UpdateType type)
    {
        var update = new Update
        {
            UpdateId = 1,
            UpdateTypeRaw = type == UpdateType.Message ? "message_created" : "message_callback",
            Message = type == UpdateType.Message ? new Message() : null,
            CallbackQuery = type == UpdateType.CallbackQuery ? new CallbackQuery { Id = "cb", From = new User { Id = 1 } } : null
        };

        var api = new Mock<IMaxBotApi>().Object;
        var options = new MaxBotOptions
        {
            Token = "token",
            BaseUrl = "https://api.max.ru/bot"
        };

        return new UpdateContext(update, api, options);
    }
}





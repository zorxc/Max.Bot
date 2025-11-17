using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Max.Bot.Configuration;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Max.Bot.Tests.Unit.Polling;

public class WebhookControllerTests
{
    [Fact]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenSignatureMissing()
    {
        // Arrange
        var (controller, _, _) = CreateController(secret: "secret");
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = await controller.PostAsync(CreateUpdate(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task PostAsync_ShouldReturnPayloadTooLarge_WhenBodyExceedsLimit()
    {
        // Arrange
        var (controller, _, _) = CreateController(secret: null);
        var context = new DefaultHttpContext();
        context.Request.ContentLength = (controller.Options.Webhook.MaxBodySizeKilobytes + 1) * 1024L;
        controller.ControllerContext = new ControllerContext { HttpContext = context };

        // Act
        var result = await controller.PostAsync(CreateUpdate(), CancellationToken.None);

        // Assert
        result.As<StatusCodeResult>().StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
    }

    [Fact]
    public async Task PostAsync_ShouldProcess_WhenSignatureValid()
    {
        // Arrange
        var secret = "secret-token";
        var (controller, pipelineMock, handlerMock) = CreateController(secret);
        var context = new DefaultHttpContext();
        context.Request.Headers[controller.Options.Webhook.SignatureHeaderName] = secret;
        context.Request.ContentLength = 100;
        controller.ControllerContext = new ControllerContext { HttpContext = context };

        // Act
        var result = await controller.PostAsync(CreateUpdate(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        pipelineMock.Verify(p => p.ProcessWebhookAsync(It.IsAny<Update>(), handlerMock.Object, context.RequestServices, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static (TestableWebhookController Controller, Mock<IUpdatePipeline> Pipeline, Mock<IUpdateHandler> Handler) CreateController(string? secret)
    {
        var pipeline = new Mock<IUpdatePipeline>();
        pipeline.Setup(p => p.ProcessWebhookAsync(It.IsAny<Update>(), It.IsAny<IUpdateHandler>(), It.IsAny<IServiceProvider?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new Mock<IUpdateHandler>();

        var options = new MaxBotOptions
        {
            Token = "token",
            BaseUrl = "https://api.max.ru/bot"
        };
        options.Webhook.SecretToken = secret;
        options.Webhook.SignatureHeaderName = "X-Test-Signature";
        options.Webhook.MaxBodySizeKilobytes = 1;

        var controller = new TestableWebhookController(pipeline.Object, handler.Object, options, Mock.Of<ILogger<WebhookController>>());
        return (controller, pipeline, handler);
    }

    private static Update CreateUpdate()
    {
        return new Update
        {
            UpdateId = 1,
            UpdateTypeRaw = "message_created",
            Message = new Message { From = new User { Username = "tester" } }
        };
    }

    private sealed class TestableWebhookController : WebhookController
    {
        public TestableWebhookController(IUpdatePipeline pipeline, IUpdateHandler handler, MaxBotOptions options, ILogger<WebhookController>? logger)
            : base(pipeline, handler, options, logger)
        {
            Options = options;
        }

        public MaxBotOptions Options { get; }
    }
}





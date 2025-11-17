using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Examples;
using Max.Bot.Examples.Samples;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;
using Moq;
using File = System.IO.File;

namespace Max.Bot.Tests.Examples;

/// <summary>
/// Ensures the sample bots keep compiling and interact with the API surface as expected.
/// </summary>
public class SampleBotsTests
{
    public static IEnumerable<object[]> SampleScenarioData()
    {
        yield return new object[]
        {
            "echo",
            Scenario(async harness =>
            {
                var handler = await harness.StartAsync();
                var update = CreateMessageUpdate("Hello world");
                await handler.HandleMessageAsync(CreateUpdateContext(harness.Api, update), CancellationToken.None);

                harness.MessagesMock.Verify(m => m.SendMessageAsync(update.Message!.Chat!.Id, "Echo: Hello world", It.IsAny<CancellationToken>()), Times.Once);
            })
        };

        yield return new object[]
        {
            "commands",
            Scenario(async harness =>
            {
                harness.ChatsMock.Setup(c => c.GetChatAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Chat { Id = 42, Title = "QA" });

                var handler = await harness.StartAsync();
                await handler.HandleMessageAsync(CreateUpdateContext(harness.Api, CreateMessageUpdate("/start")), CancellationToken.None);
                await handler.HandleMessageAsync(CreateUpdateContext(harness.Api, CreateMessageUpdate("/stats")), CancellationToken.None);

                harness.MessagesMock.Verify(m => m.SendMessageAsync(It.IsAny<long>(), It.Is<string>(text => text.Contains("Welcome")), It.IsAny<CancellationToken>()), Times.Once);
                harness.MessagesMock.Verify(m => m.SendMessageAsync(It.IsAny<long>(), It.Is<string>(text => text.Contains("QA")), It.IsAny<CancellationToken>()), Times.Once);
            })
        };

        yield return new object[]
        {
            "keyboard",
            Scenario(async harness =>
            {
                harness.MessagesMock.Setup(m => m.SendMessageAsync(
                        It.IsAny<SendMessageRequest>(),
                        It.IsAny<long?>(),
                        It.IsAny<long?>(),
                        It.IsAny<bool?>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Message());

                harness.MessagesMock.Setup(m => m.AnswerCallbackQueryAsync(It.IsAny<string>(), It.IsAny<AnswerCallbackQueryRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Response());

                var handler = await harness.StartAsync();
                await handler.HandleMessageAsync(CreateUpdateContext(harness.Api, CreateMessageUpdate("/buttons")), CancellationToken.None);

                harness.MessagesMock.Verify(m => m.SendMessageAsync(
                    It.Is<SendMessageRequest>(request => request.Attachments != null && request.Attachments.Length > 0),
                    1337,
                    null,
                    null,
                    It.IsAny<CancellationToken>()), Times.Once);

                var callbackUpdate = new Update
                {
                    UpdateId = 2,
                    UpdateTypeRaw = "message_callback",
                    CallbackQuery = new CallbackQuery
                    {
                        Id = "cb-1",
                        Data = "vote:approve"
                    }
                };

                await handler.HandleCallbackQueryAsync(CreateUpdateContext(harness.Api, callbackUpdate), CancellationToken.None);
                harness.MessagesMock.Verify(m => m.AnswerCallbackQueryAsync("cb-1", It.IsAny<AnswerCallbackQueryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            })
        };

        yield return new object[]
        {
            "files",
            Scenario(async harness =>
            {
                using var tempFile = new TempFile();
                await File.WriteAllTextAsync(tempFile.Path, "payload");

                harness.FilesMock.Setup(f => f.UploadFileAsync(UploadType.File, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UploadResponse { Url = "https://upload" });
                harness.FilesMock.Setup(f => f.UploadFileDataAsync("https://upload", It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new { token = "file-token" });

                harness.MessagesMock.Setup(m => m.SendMessageWithAttachmentAsync(It.IsAny<AttachmentRequest>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<TextFormat?>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Message());

                harness.WithUploadFile(tempFile.Path);

                var handler = await harness.StartAsync();
                await handler.HandleMessageAsync(CreateUpdateContext(harness.Api, CreateMessageUpdate("/file")), CancellationToken.None);

                harness.FilesMock.Verify(f => f.UploadFileAsync(UploadType.File, It.IsAny<CancellationToken>()), Times.Once);
                harness.FilesMock.Verify(f => f.UploadFileDataAsync("https://upload", It.IsAny<Stream>(), Path.GetFileName(tempFile.Path), It.IsAny<CancellationToken>()), Times.Once);
                harness.MessagesMock.Verify(m => m.SendMessageWithAttachmentAsync(It.IsAny<AttachmentRequest>(), 1337, null, It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<TextFormat?>(), It.IsAny<CancellationToken>()), Times.Once);
            })
        };
    }

    [Fact]
    public void SampleRegistry_ShouldExposeAllSamples()
    {
        SampleRegistry.AvailableSamples.Should().BeEquivalentTo(new[] { "echo", "commands", "keyboard", "files" });
    }

    [Theory]
    [MemberData(nameof(SampleScenarioData))]
    public async Task Samples_ShouldHandlePrimaryScenario(string sampleName, Func<IBotSample, Task> scenario)
    {
        SampleRegistry.TryGet(sampleName, out var sample).Should().BeTrue();

        await scenario(sample!);
    }

    private static Func<IBotSample, Task> Scenario(Func<SampleScenarioHarness, Task> runner)
    {
        return async sample =>
        {
            await using var harness = SampleScenarioHarness.Create(sample);
            await runner(harness);
        };
    }

    private static Update CreateMessageUpdate(string text)
    {
        return new Update
        {
            UpdateId = 1,
            UpdateTypeRaw = "message_created",
            Message = new Message
            {
                Chat = new Chat { Id = 1337, Title = "SampleChat" },
                Body = new MessageBody { Text = text },
                Text = text
            }
        };
    }

    private static UpdateContext CreateUpdateContext(IMaxBotApi api, Update update)
    {
        return new UpdateContext(update, api, new MaxBotOptions { Token = "TEST_TOKEN" });
    }

    private sealed class SampleScenarioHarness : IAsyncDisposable
    {
        private readonly IBotSample _sample;
        private readonly CancellationTokenSource _cts = new();
        private Task? _runTask;
        private SampleSettings _settings;

        private SampleScenarioHarness(
            IBotSample sample,
            LoopbackSampleRuntime runtime,
            SampleExecutionContext context,
            Mock<IMaxBotApi> apiMock,
            Mock<IMessagesApi> messagesMock,
            Mock<IChatsApi> chatsMock,
            Mock<IFilesApi> filesMock,
            Mock<ISubscriptionsApi> subscriptionsMock)
        {
            _sample = sample;
            Runtime = runtime;
            Context = context;
            ApiMock = apiMock;
            MessagesMock = messagesMock;
            ChatsMock = chatsMock;
            FilesMock = filesMock;
            SubscriptionsMock = subscriptionsMock;
            _settings = context.Settings;
        }

        public static SampleScenarioHarness Create(IBotSample sample)
        {
            var apiMock = new Mock<IMaxBotApi>();
            var messagesMock = new Mock<IMessagesApi>();
            var chatsMock = new Mock<IChatsApi>();
            var filesMock = new Mock<IFilesApi>();
            var subscriptionsMock = new Mock<ISubscriptionsApi>();

            apiMock.SetupGet(x => x.Messages).Returns(messagesMock.Object);
            apiMock.SetupGet(x => x.Bot).Returns(Mock.Of<IBotApi>());
            apiMock.SetupGet(x => x.Chats).Returns(chatsMock.Object);
            apiMock.SetupGet(x => x.Users).Returns(Mock.Of<IUsersApi>());
            apiMock.SetupGet(x => x.Files).Returns(filesMock.Object);
            apiMock.SetupGet(x => x.Subscriptions).Returns(subscriptionsMock.Object);

            var runtime = new LoopbackSampleRuntime(apiMock.Object);
            var settings = new SampleSettings("TEST_TOKEN", 1337, null);
            var context = new SampleExecutionContext(runtime, settings, TextWriter.Null, TextWriter.Null);

            return new SampleScenarioHarness(sample, runtime, context, apiMock, messagesMock, chatsMock, filesMock, subscriptionsMock);
        }

        public LoopbackSampleRuntime Runtime { get; }

        public SampleExecutionContext Context { get; private set; }

        public IMaxBotApi Api => ApiMock.Object;

        public Mock<IMaxBotApi> ApiMock { get; }

        public Mock<IMessagesApi> MessagesMock { get; }

        public Mock<IChatsApi> ChatsMock { get; }

        public Mock<IFilesApi> FilesMock { get; }

        public Mock<ISubscriptionsApi> SubscriptionsMock { get; }

        public void WithUploadFile(string path)
        {
            _settings = new SampleSettings(_settings.Token, _settings.DefaultChatId, path);
            Context = new SampleExecutionContext(Runtime, _settings, Context.Output, Context.Error);
        }

        public async Task<IUpdateHandler> StartAsync()
        {
            _runTask = _sample.RunAsync(Context, _cts.Token);
            return await Runtime.WaitForHandlerAsync();
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            if (_runTask != null)
            {
                await _runTask.WaitAsync(TimeSpan.FromSeconds(2));
            }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            _cts.Dispose();
        }
    }

    private sealed class TempFile : IDisposable
    {
        public TempFile()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"max-bot-{Guid.NewGuid():N}.tmp");
            System.IO.File.WriteAllText(Path, string.Empty);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}



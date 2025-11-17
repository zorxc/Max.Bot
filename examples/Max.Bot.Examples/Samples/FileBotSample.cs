using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Polling;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;

namespace Max.Bot.Examples.Samples;

/// <summary>
/// Demonstrates how to handle the file upload flow exposed by the Max API.
/// </summary>
public sealed class FileBotSample : IBotSample
{
    /// <inheritdoc />
    public string Name => "files";

    /// <inheritdoc />
    public string Description => "Uploads a local file and sends it when the /file command is received.";

    /// <inheritdoc />
    public Task RunAsync(SampleExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var handler = new DelegatingUpdateHandler(
            onMessage: (updateContext, ct) => HandleMessageAsync(context, updateContext, ct));

        return SampleUtilities.RunPollingLoopAsync(context, handler, cancellationToken);
    }

    private static async Task HandleMessageAsync(SampleExecutionContext sampleContext, UpdateContext context, CancellationToken cancellationToken)
    {
        var message = context.Update.Message;
        var chatId = SampleUtilities.GetChatId(message);
        var text = SampleUtilities.GetNormalizedText(message);

        if (!chatId.HasValue || !string.Equals(text, "/file", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sampleContext.Settings.UploadFilePath))
        {
            await context.Api.Messages.SendMessageAsync(
                chatId.Value,
                "Configure MAX_BOT_FILE to point to a sample file before using /file.",
                cancellationToken).ConfigureAwait(false);
            return;
        }

        var attachment = await BuildAttachmentAsync(context, sampleContext.Settings.UploadFilePath, cancellationToken).ConfigureAwait(false);
        await context.Api.Messages.SendMessageWithAttachmentAsync(
            attachment,
            chatId: chatId.Value,
            text: "Here is your file upload СЂСџвЂњР‹",
            cancellationToken: cancellationToken).ConfigureAwait(false);
        sampleContext.Output.WriteLine($"File '{sampleContext.Settings.UploadFilePath}' uploaded for chat {chatId.Value}.");
    }

    private static async Task<AttachmentRequest> BuildAttachmentAsync(UpdateContext context, string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        var uploadResponse = await context.Api.Files.UploadFileAsync(UploadType.File, cancellationToken).ConfigureAwait(false);
        var payload = await context.Api.Files.UploadFileDataAsync(
            uploadResponse.Url,
            stream,
            Path.GetFileName(filePath),
            cancellationToken).ConfigureAwait(false);

        return new AttachmentRequest
        {
            Type = "file",
            Payload = payload
        };
    }
}




using System;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Requests;

namespace Max.Bot.Examples.Samples;

/// <summary>
/// Sample showcasing inline keyboards and callback query processing.
/// </summary>
public sealed class KeyboardBotSample : IBotSample
{
    /// <inheritdoc />
    public string Name => "keyboard";

    /// <inheritdoc />
    public string Description => "Sends inline keyboards and responds to button presses.";

    /// <inheritdoc />
    public Task RunAsync(SampleExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var handler = new DelegatingUpdateHandler(
            onMessage: (updateContext, ct) => HandleMessageAsync(context, updateContext, ct),
            onCallback: (updateContext, ct) => HandleCallbackAsync(context, updateContext, ct));

        return SampleUtilities.RunPollingLoopAsync(context, handler, cancellationToken);
    }

    private static async Task HandleMessageAsync(SampleExecutionContext sampleContext, UpdateContext context, CancellationToken cancellationToken)
    {
        var message = context.Update.Message;
        var chatId = SampleUtilities.GetChatId(message);
        var text = SampleUtilities.GetNormalizedText(message);

        if (!chatId.HasValue || !string.Equals(text, "/buttons", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var keyboard = new InlineKeyboard(new[]
        {
            new[]
            {
                new InlineKeyboardButton { Text = "СЂСџвЂРЊ Approve", CallbackData = "vote:approve" },
                new InlineKeyboardButton { Text = "СЂСџвЂР‹ Reject", CallbackData = "vote:reject" }
            },
            new[]
            {
                new InlineKeyboardButton { Text = "СЂСџвЂњС™ Docs", Url = "https://dev.max.ru/docs-api" }
            }
        });

        var request = new SendMessageRequest
        {
            Text = "Choose an option:",
            Attachments = new[]
            {
                new AttachmentRequest
                {
                    Type = "inline_keyboard",
                    Payload = keyboard
                }
            }
        };

        await context.Api.Messages.SendMessageAsync(
            request,
            chatId: chatId.Value,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        sampleContext.Output.WriteLine("Inline keyboard sent.");
    }

    private static async Task HandleCallbackAsync(SampleExecutionContext sampleContext, UpdateContext context, CancellationToken cancellationToken)
    {
        var callback = context.Update.CallbackQuery;
        if (callback == null)
        {
            return;
        }

        var responseText = callback.Data switch
        {
            "vote:approve" => "РІСљвЂ¦ Approved",
            "vote:reject" => "РІСњРЉ Rejected",
            _ => "РІвЂћв„–РїС‘РЏ Received."
        };

        var request = new AnswerCallbackQueryRequest
        {
            Notification = responseText
        };

        await context.Api.Messages.AnswerCallbackQueryAsync(callback.Id, request, cancellationToken).ConfigureAwait(false);
        sampleContext.Output.WriteLine($"Callback '{callback.Data}' processed.");
    }
}




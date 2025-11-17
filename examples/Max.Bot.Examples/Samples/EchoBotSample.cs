using System;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Polling;

namespace Max.Bot.Examples.Samples;

/// <summary>
/// Demonstrates how to build the classic echo bot using <see cref="Max.Bot.MaxClient"/>.
/// </summary>
public sealed class EchoBotSample : IBotSample
{
    /// <inheritdoc />
    public string Name => "echo";

    /// <inheritdoc />
    public string Description => "Echoes back every incoming text message.";

    /// <inheritdoc />
    public Task RunAsync(SampleExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var handler = new DelegatingUpdateHandler(
            onMessage: async (updateContext, ct) =>
            {
                var message = updateContext.Update.Message;
                var chatId = SampleUtilities.GetChatId(message);
                var text = SampleUtilities.GetNormalizedText(message);

                if (!chatId.HasValue || string.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                var reply = $"Echo: {text}";
                await updateContext.Api.Messages.SendMessageAsync(chatId.Value, reply, ct).ConfigureAwait(false);
                context.Output.WriteLine($"Echoed message '{text}' in chat {chatId.Value}.");
            });

        return SampleUtilities.RunPollingLoopAsync(context, handler, cancellationToken);
    }
}




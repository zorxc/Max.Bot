using System;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;

namespace Max.Bot.Examples.Samples;

/// <summary>
/// Sample showcasing contact handling.
/// Demonstrates how to extract phone numbers and contact information from contact attachments.
/// </summary>
public sealed class ContactBotSample : IBotSample
{
    /// <inheritdoc />
    public string Name => "contact";

    /// <inheritdoc />
    public string Description => "Handles contact sharing and extracts phone numbers.";

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

        if (!chatId.HasValue)
        {
            return;
        }

        // Check if message has attachments
#pragma warning disable CS8602
        var attachments = message.Body?.Attachments! ?? Array.Empty<Attachment>();
#pragma warning restore CS8602
        if (attachments.Length == 0)
        {
            return;
        }

        // Look for contact attachment
        foreach (var attachment in attachments)
        {
            if (attachment is ContactAttachment contactAttachment)
            {
                await HandleContactAsync(sampleContext, context, contactAttachment, chatId.Value, cancellationToken);
                return;
            }
        }
    }

    private static async Task HandleContactAsync(
        SampleExecutionContext sampleContext,
        UpdateContext context,
        ContactAttachment contactAttachment,
        long chatId,
        CancellationToken cancellationToken)
    {
        // Method 1: Use helper properties (recommended)
        var phoneNumber = contactAttachment.PhoneNumber;
        var fullName = contactAttachment.FullName;

        // Method 2: Access raw data
        var vcfInfo = contactAttachment.Payload?.VcfInfo;
        var maxInfo = contactAttachment.Payload?.MaxInfo;

        // Method 3: Use ContactHelpers directly
        var parsedPhoneNumber = ContactHelpers.GetPhoneNumber(contactAttachment.Payload);
        var parsedFullName = ContactHelpers.GetFullName(contactAttachment.Payload);

        var response = $"""
            РќРѕРІС‹Р№ РєРѕРЅС‚Р°РєС‚ РїРѕР»СѓС‡РµРЅ! РІСќСџС'С‘

            РўРµР»РµС„РѕРЅ: {phoneNumber ?? "РќРµ СѓРєР°Р·Р°РЅ"}
            РРјСЏ: {fullName ?? "РќРµ СѓРєР°Р·Р°РЅРѕ"}

            Р”РѕРїРѕР»РЅРёС‚РµР»СЊРЅР°СЏ РёРЅС„РѕСЂРјР°С†РёСЏ:
            - User ID: {maxInfo?.Id}
            - Username: {maxInfo?.Username ?? "РќРµС‚"}
            - Bot: {(maxInfo?.IsBot == true ? "Р”Р°" : "РќРµС‚")}
            """;

        await context.Api.Messages.SendMessageAsync(
            chatId,
            response,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        sampleContext.Output.WriteLine($"Contact received: {phoneNumber}, {fullName}");
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to pin a message in a chat.
/// </summary>
public class PinMessageRequest
{
    /// <summary>
    /// Gets or sets the message ID to pin.
    /// </summary>
    /// <value>
    /// The message ID to pin. Corresponds to the Message.body.mid field.
    /// </value>
    [Required(ErrorMessage = "Message ID is required.")]
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to notify chat participants.
    /// </summary>
    /// <value>
    /// True to notify participants with a system message about pinning (default: true); otherwise, false.
    /// If null, uses default value (true).
    /// </value>
    [JsonPropertyName("notify")]
    public bool? Notify { get; set; }
}





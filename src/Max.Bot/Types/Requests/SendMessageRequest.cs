using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to send a message with full support for all parameters.
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// Gets or sets the text content of the message.
    /// </summary>
    /// <value>The text content, or null if not applicable.</value>
    [StringLength(4000, ErrorMessage = "Text must not exceed 4000 characters.")]
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the attachments for the message.
    /// </summary>
    /// <value>
    /// The attachments array. Each attachment must have a type (image, video, audio, file)
    /// and a payload object containing attachment data.
    /// </value>
    [JsonPropertyName("attachments")]
    public AttachmentRequest[]? Attachments { get; set; }

    /// <summary>
    /// Gets or sets the link to a message for forwarding or replying.
    /// </summary>
    /// <value>The linked message information, or null if not applicable.</value>
    [JsonPropertyName("link")]
    public NewMessageLink? Link { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify chat participants.
    /// </summary>
    /// <value>True to notify participants (default); otherwise, false.</value>
    [JsonPropertyName("notify")]
    public bool? Notify { get; set; }

    /// <summary>
    /// Gets or sets the text format for the message content.
    /// </summary>
    /// <value>The text format (markdown or html), or null for plain text.</value>
    [JsonPropertyName("format")]
    public TextFormat? Format { get; set; }
}


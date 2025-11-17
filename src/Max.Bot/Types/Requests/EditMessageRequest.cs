using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to edit a message.
/// </summary>
public class EditMessageRequest
{
    /// <summary>
    /// Gets or sets the new text content of the message.
    /// </summary>
    /// <value>The new text content, or null to keep the existing text.</value>
    [StringLength(4000, ErrorMessage = "Text must not exceed 4000 characters.")]
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the attachments for the message.
    /// </summary>
    /// <value>
    /// The attachments array. If null, existing attachments are not changed.
    /// If empty array, all attachments will be removed.
    /// </value>
    [JsonPropertyName("attachments")]
    public Attachment[]? Attachments { get; set; }

    /// <summary>
    /// Gets or sets the link to a message (forwarded or reply).
    /// </summary>
    /// <value>The linked message, or null if not applicable.</value>
    [JsonPropertyName("link")]
    public Message? Link { get; set; }

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


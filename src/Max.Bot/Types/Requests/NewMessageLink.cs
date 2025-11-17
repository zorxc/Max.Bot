using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a link to a message for forwarding or replying.
/// </summary>
public class NewMessageLink
{
    /// <summary>
    /// Gets or sets the unique identifier of the message to link to.
    /// </summary>
    /// <value>The unique identifier of the message.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Message ID must be greater than zero.")]
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the chat containing the message.
    /// </summary>
    /// <value>The unique identifier of the chat, or null if not specified.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Chat ID must be greater than zero.")]
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }
}


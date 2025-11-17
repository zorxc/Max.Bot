using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types;

/// <summary>
/// Represents the recipient of a message (can be a chat or user).
/// </summary>
public class MessageRecipient
{
    /// <summary>
    /// Gets or sets the chat ID of the recipient.
    /// </summary>
    /// <value>The unique identifier of the chat.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Chat ID must be greater than zero.")]
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// Gets or sets the type of the chat.
    /// </summary>
    /// <value>The type of the chat (e.g., "dialog", "group", "channel").</value>
    [JsonPropertyName("chat_type")]
    public string? ChatType { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the recipient (for dialogs).
    /// </summary>
    /// <value>The unique identifier of the user.</value>
    [Range(1, long.MaxValue, ErrorMessage = "User ID must be greater than zero.")]
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }
}


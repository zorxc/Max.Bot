using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Converters;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types;

/// <summary>
/// Represents a Max Messenger message.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the unique identifier of the message.
    /// </summary>
    /// <value>The unique identifier of the message.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Message ID must be greater than zero.")]
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the chat where the message was sent.
    /// </summary>
    /// <value>The chat where the message was sent, or null if not available.</value>
    [JsonPropertyName("chat")]
    public Chat? Chat { get; set; }

    /// <summary>
    /// Gets or sets the user who sent the message.
    /// </summary>
    /// <value>The user who sent the message, or null if not available.</value>
    [JsonPropertyName("from")]
    public User? From { get; set; }

    /// <summary>
    /// Gets or sets the user who sent the message (alternative property name from API documentation).
    /// </summary>
    /// <value>The user who sent the message, or null if not available.</value>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Gets or sets the recipient of the message. Can be a user or a chat.
    /// </summary>
    /// <value>The recipient of the message, or null if not available.</value>
    [JsonPropertyName("recipient")]
    public MessageRecipient? Recipient { get; set; }

    /// <summary>
    /// Gets or sets the text content of the message.
    /// </summary>
    /// <value>The text content of the message, or null if not available.</value>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the date when the message was sent (Unix timestamp).
    /// </summary>
    /// <value>The date when the message was sent.</value>
    [JsonPropertyName("date")]
    [JsonConverter(typeof(UnixTimestampJsonConverter))]
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the message was created (Unix timestamp).
    /// </summary>
    /// <value>The timestamp when the message was created.</value>
    [Range(0, long.MaxValue, ErrorMessage = "Timestamp cannot be negative.")]
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the type of the message.
    /// </summary>
    /// <value>The type of the message (text, image, or file).</value>
    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    /// <summary>
    /// Gets or sets the linked message (forwarded or reply message).
    /// </summary>
    /// <value>The linked message, or null if not available.</value>
    [JsonPropertyName("link")]
    public Message? Link { get; set; }

    /// <summary>
    /// Gets or sets the body of the message (text and attachments).
    /// </summary>
    /// <value>The body of the message, or null if not available.</value>
    [JsonPropertyName("body")]
    public MessageBody? Body { get; set; }

    /// <summary>
    /// Gets or sets the statistics of the message.
    /// </summary>
    /// <value>The statistics of the message, or null if not available.</value>
    [JsonPropertyName("stat")]
    public MessageStat? Stat { get; set; }

    /// <summary>
    /// Gets or sets the public URL of the message.
    /// </summary>
    /// <value>The public URL of the message, or null if not available (e.g., for dialogs or non-public chats).</value>
    [Url(ErrorMessage = "Url must be a valid URL.")]
    [StringLength(512, ErrorMessage = "Url must not exceed 512 characters.")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Represents the body of a message containing text and attachments.
/// </summary>
public class MessageBody
{
    /// <summary>
    /// Gets or sets the message ID.
    /// </summary>
    /// <value>The unique identifier of the message.</value>
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    /// <summary>
    /// Gets or sets the sequence number of the message.
    /// </summary>
    /// <value>The sequence number for ordering messages.</value>
    [Range(0, long.MaxValue, ErrorMessage = "Sequence number cannot be negative.")]
    [JsonPropertyName("seq")]
    public long? Seq { get; set; }

    /// <summary>
    /// Gets or sets the text content of the message.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the attachments of the message.
    /// </summary>
    [JsonPropertyName("attachments")]
    public Attachment[]? Attachments { get; set; }
}

/// <summary>
/// Represents statistics of a message.
/// </summary>
public class MessageStat
{
    /// <summary>
    /// Gets or sets the number of times the message was read.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "ReadCount cannot be negative.")]
    [JsonPropertyName("read_count")]
    public int? ReadCount { get; set; }
}


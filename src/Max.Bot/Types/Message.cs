using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Converters;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types;

/// <summary>
/// Represents a message in Max Messenger chat.
/// Maps to the Message object in the official Max API documentation.
/// </summary>
public class Message
{
    private string? _text;

    /// <summary>
    /// Gets or sets the user who sent the message.
    /// Maps to "sender" field from API.
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Gets or sets the user who sent the message.
    /// Backward compatibility property that syncs with Sender.
    /// </summary>
    [JsonPropertyName("from")]
    [Obsolete("Use Sender property instead. This property will be removed in future versions.")]
    public User? From
    {
        get => Sender;
        set => Sender = value;
    }

    /// <summary>
    /// Gets or sets the recipient of the message.
    /// Can be a user or a chat.
    /// </summary>
    [JsonPropertyName("recipient")]
    public MessageRecipient? Recipient { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the message was created (Unix timestamp in milliseconds).
    /// Maps to "timestamp" field from API.
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "Timestamp cannot be negative.")]
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the linked message (forwarded or reply message).
    /// Maps to "link" field from API.
    /// </summary>
    [JsonPropertyName("link")]
    public LinkedMessage? Link { get; set; }

    /// <summary>
    /// Gets or sets the body of the message (text and attachments).
    /// Can be null if message contains only forwarded message.
    /// Maps to "body" field from API.
    /// </summary>
    [JsonPropertyName("body")]
    public MessageBody? Body { get; set; }

    /// <summary>
    /// Gets or sets the statistics of the message.
    /// Maps to "stat" field from API.
    /// </summary>
    [JsonPropertyName("stat")]
    public MessageStat? Stat { get; set; }

    /// <summary>
    /// Gets or sets the public URL of the message.
    /// Can be null for dialogs or non-public chats.
    /// Maps to "url" field from API.
    /// </summary>
    [StringLength(512, ErrorMessage = "Url must not exceed 512 characters.")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the text content of the message.
    /// When getting, returns Body.Text if available, otherwise the internal text field.
    /// When setting, sets the internal text field and Body.Text if Body exists.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text
    {
        get => Body?.Text ?? _text;
        set
        {
            _text = value;
            if (Body != null)
            {
                Body.Text = value;
            }
        }
    }

    /// <summary>
    /// Gets the message ID from Body.Mid.
    /// </summary>
    [JsonIgnore]
    public string? Mid => Body?.Mid;

    #region Backward Compatibility

    /// <summary>
    /// Gets or sets the unique identifier of the message.
    /// Note: This field is not part of the official API and may be removed.
    /// Use Body.Mid for the message ID.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "Message ID must be greater than zero.")]
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    /// <summary>
    /// Gets or sets the chat where the message was sent.
    /// Note: This field is not part of the official API Message object.
    /// Use Recipient for message recipient information.
    /// </summary>
    [JsonPropertyName("chat")]
    public Chat? Chat { get; set; }

    /// <summary>
    /// Gets or sets the date when the message was sent.
    /// Note: This field is not part of the official API. Use Timestamp instead.
    /// </summary>
    [JsonPropertyName("date")]
    [JsonConverter(typeof(UnixTimestampJsonConverter))]
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the type of the message.
    /// Note: This field is not part of the official API.
    /// </summary>
    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    #endregion
}

/// <summary>
/// Represents a linked (forwarded or reply) message.
/// Maps to LinkedMessage in the official Max API documentation.
/// </summary>
public class LinkedMessage
{
    /// <summary>
    /// Gets or sets the type of link.
    /// Possible values: "forward", "reply".
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the user who sent the original message.
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Gets or sets the chat where the message was sent.
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    /// <summary>
    /// Gets or sets the message ID for backward compatibility.
    /// </summary>
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    /// <summary>
    /// Gets the text content of the linked message.
    /// Returns Message.Text if available.
    /// </summary>
    [JsonIgnore]
    public string? Text => Message?.Text;
}

/// <summary>
/// Represents the body of a message containing text and attachments.
/// Maps to MessageBody in the official Max API documentation.
/// </summary>
public class MessageBody
{
    /// <summary>
    /// Gets or sets the message ID.
    /// </summary>
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    /// <summary>
    /// Gets or sets the sequence number of the message.
    /// Used for ordering messages.
    /// </summary>
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
/// Maps to MessageStat in the official Max API documentation.
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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types;

/// <summary>
/// Represents a Max Messenger chat.
/// </summary>
public class Chat
{
    /// <summary>
    /// Gets or sets the unique identifier of the chat.
    /// </summary>
    /// <value>The unique identifier of the chat.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Chat ID must be greater than zero.")]
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the chat (alternative property name from API documentation).
    /// </summary>
    /// <value>The unique identifier of the chat.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Chat ID must be greater than zero.")]
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// Gets or sets the type of the chat.
    /// </summary>
    /// <value>The type of the chat (private, group, or channel).</value>
    [JsonPropertyName("type")]
    public ChatType Type { get; set; }

    /// <summary>
    /// Gets or sets the status of the chat.
    /// </summary>
    /// <value>The status of the chat (active, removed, left, or closed).</value>
    [JsonPropertyName("status")]
    public ChatStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the title of the chat (for groups and channels).
    /// </summary>
    /// <value>The title of the chat, or null if not available.</value>
    [StringLength(256, ErrorMessage = "Title must not exceed 256 characters.")]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the username of the chat (for private chats and channels).
    /// </summary>
    /// <value>The username of the chat, or null if not available.</value>
    [StringLength(64, ErrorMessage = "Username must not exceed 64 characters.")]
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the first name of the chat (for private chats).
    /// </summary>
    /// <value>The first name of the chat, or null if not available.</value>
    [StringLength(64, ErrorMessage = "First name must not exceed 64 characters.")]
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the chat (for private chats).
    /// </summary>
    /// <value>The last name of the chat, or null if not available.</value>
    [StringLength(64, ErrorMessage = "Last name must not exceed 64 characters.")]
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the icon of the chat.
    /// </summary>
    /// <value>The icon of the chat, or null if not available.</value>
    [JsonPropertyName("icon")]
    public Photo? Icon { get; set; }

    /// <summary>
    /// Gets or sets the time of the last event in the chat (Unix timestamp).
    /// </summary>
    /// <value>The time of the last event in the chat.</value>
    [Range(0, long.MaxValue, ErrorMessage = "LastEventTime cannot be negative.")]
    [JsonPropertyName("last_event_time")]
    public long? LastEventTime { get; set; }

    /// <summary>
    /// Gets or sets the number of participants in the chat.
    /// </summary>
    /// <value>The number of participants in the chat. Always 2 for dialogs.</value>
    [Range(1, int.MaxValue, ErrorMessage = "ParticipantsCount must be greater than zero.")]
    [JsonPropertyName("participants_count")]
    public int? ParticipantsCount { get; set; }

    /// <summary>
    /// Gets or sets the ID of the chat owner.
    /// </summary>
    /// <value>The ID of the chat owner, or null if not available.</value>
    [Range(1, long.MaxValue, ErrorMessage = "OwnerId must be greater than zero.")]
    [JsonPropertyName("owner_id")]
    public long? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the participants of the chat with their last activity time.
    /// </summary>
    /// <value>The participants of the chat, or null if not available (e.g., when requesting a list of chats).</value>
    [JsonPropertyName("participants")]
    public object? Participants { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the chat is public.
    /// </summary>
    /// <value>True if the chat is public; otherwise, false. Always false for dialogs.</value>
    [JsonPropertyName("is_public")]
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets the link to the chat.
    /// </summary>
    /// <value>The link to the chat, or null if not available.</value>
    [Url(ErrorMessage = "Link must be a valid URL.")]
    [StringLength(512, ErrorMessage = "Link must not exceed 512 characters.")]
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>
    /// Gets or sets the description of the chat.
    /// </summary>
    /// <value>The description of the chat, or null if not available.</value>
    [StringLength(512, ErrorMessage = "Description must not exceed 512 characters.")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the user data in the dialog (only for chats of type "dialog").
    /// </summary>
    /// <value>The user data in the dialog, or null if not available.</value>
    [JsonPropertyName("dialog_with_user")]
    public User? DialogWithUser { get; set; }

    /// <summary>
    /// Gets or sets the ID of the message containing the button that initiated the chat.
    /// </summary>
    /// <value>The ID of the message, or null if not available.</value>
    [StringLength(256, ErrorMessage = "ChatMessageId must not exceed 256 characters.")]
    [JsonPropertyName("chat_message_id")]
    public string? ChatMessageId { get; set; }

    /// <summary>
    /// Gets or sets the pinned message in the chat.
    /// </summary>
    /// <value>The pinned message in the chat, or null if not available (returned only when requesting a specific chat).</value>
    [JsonPropertyName("pinned_message")]
    public Message? PinnedMessage { get; set; }
}


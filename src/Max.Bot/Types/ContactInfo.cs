using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents a contact information.
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the contact.
    /// </summary>
    /// <value>The unique identifier of the contact.</value>
    [JsonPropertyName("user_id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the username of the contact.
    /// </summary>
    /// <value>The username of the contact, or null if not available.</value>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the first name of the contact.
    /// </summary>
    /// <value>The first name of the contact, or null if not available.</value>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the contact.
    /// </summary>
    /// <value>The last name of the contact, or null if not available.</value>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the contact is a bot.
    /// </summary>
    /// <value>True if the user is a bot; otherwise, false.</value>
    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    /// <summary>
    /// Gets or sets the last activity time of the contact (Unix timestamp in milliseconds).
    /// </summary>
    /// <value>The timestamp of the user's last activity.</value>
    [JsonPropertyName("last_activity_time")]
    public long? LastActivityTime { get; set; }
}

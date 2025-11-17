using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to set a webhook.
/// </summary>
public class SetWebhookRequest
{
    /// <summary>
    /// Gets or sets the URL where updates will be sent.
    /// </summary>
    /// <value>The webhook URL (must be HTTPS).</value>
    [Required]
    [Url(ErrorMessage = "URL must be a valid HTTPS URL.")]
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of update types to receive.
    /// </summary>
    /// <value>List of update types (e.g., "message_created", "bot_started").</value>
    [JsonPropertyName("update_types")]
    public List<string>? UpdateTypes { get; set; }

    /// <summary>
    /// Gets or sets the secret that will be sent in the X-Max-Bot-Api-Secret header.
    /// </summary>
    /// <value>The secret string (5-256 characters, A-Z, a-z, 0-9, hyphen, underscore).</value>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}





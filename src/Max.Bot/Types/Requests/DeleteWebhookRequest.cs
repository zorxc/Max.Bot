using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to delete a webhook.
/// </summary>
public class DeleteWebhookRequest
{
    /// <summary>
    /// Gets or sets the URL to remove from webhook subscriptions.
    /// </summary>
    /// <value>The webhook URL to delete.</value>
    [Required]
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;
}





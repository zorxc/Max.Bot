using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents information about a webhook.
/// </summary>
public class WebhookInfo
{
    /// <summary>
    /// Gets or sets the URL where updates are sent.
    /// </summary>
    /// <value>The webhook URL, or null if no webhook is set.</value>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there are pending updates.
    /// </summary>
    /// <value>True if there are pending updates; otherwise, false.</value>
    [JsonPropertyName("pending_update_count")]
    public int? PendingUpdateCount { get; set; }
}







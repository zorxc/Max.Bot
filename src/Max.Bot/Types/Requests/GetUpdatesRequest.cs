using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to get updates.
/// </summary>
public class GetUpdatesRequest
{
    /// <summary>
    /// Gets or sets the maximum number of updates to retrieve.
    /// </summary>
    /// <value>The maximum number of updates (1-1000, default: 100).</value>
    [Range(1, 1000, ErrorMessage = "Limit must be between 1 and 1000.")]
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the timeout in seconds for long polling.
    /// </summary>
    /// <value>The timeout in seconds (0-90, default: 30).</value>
    [Range(0, 90, ErrorMessage = "Timeout must be between 0 and 90 seconds.")]
    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the marker for pagination.
    /// </summary>
    /// <value>The marker for the next page, or null to get all new updates.</value>
    [JsonPropertyName("marker")]
    public long? Marker { get; set; }

    /// <summary>
    /// Gets or sets the list of update types to filter.
    /// </summary>
    /// <value>The list of update types (e.g., "message_created", "message_callback"), or null to get all types.</value>
    [JsonPropertyName("types")]
    public List<string>? Types { get; set; }
}







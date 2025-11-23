using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents the response from the GetUpdates endpoint.
/// </summary>
public class GetUpdatesResponse
{
    /// <summary>
    /// Gets or sets the list of updates.
    /// </summary>
    /// <value>The list of updates.</value>
    [JsonPropertyName("updates")]
    public Update[] Updates { get; set; } = default!;

    /// <summary>
    /// Gets or sets the marker for the next page of updates.
    /// </summary>
    /// <value>The marker for the next page, or null if no more updates.</value>
    [JsonPropertyName("marker")]
    public long? Marker { get; set; }
}







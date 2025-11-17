using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents an API error response.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful (always false for errors).
    /// </summary>
    /// <value>False for error responses.</value>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>
    /// Gets or sets the error information.
    /// </summary>
    /// <value>The error information, or null if not available.</value>
    [JsonPropertyName("error")]
    public Error? Error { get; set; }
}


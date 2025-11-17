using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents a successful API response with data.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class Response<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    /// <value>True if the request was successful; otherwise, false.</value>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    /// <value>The response data, or null if not available.</value>
    [JsonPropertyName("result")]
    public T? Result { get; set; }
}

/// <summary>
/// Represents a simple API response with success status and optional message.
/// </summary>
public class Response
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    /// <value>True if the request was successful; otherwise, false.</value>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    /// <value>The response message, or null if not available.</value>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}


using System.Net.Http;
using System.Text;

namespace Max.Bot.Networking;

/// <summary>
/// Represents a request to the Max Bot API.
/// </summary>
public class MaxApiRequest
{
    /// <summary>
    /// Gets or sets the HTTP method for the request.
    /// </summary>
    /// <value>The HTTP method (GET, POST, PUT, DELETE, etc.).</value>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Gets or sets the endpoint (relative path) for the request.
    /// </summary>
    /// <value>The endpoint relative to the base URL (e.g., "/me", "/messages").</value>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request body object to be serialized to JSON.
    /// </summary>
    /// <value>The object to serialize as the request body. Can be null if the request doesn't have a body.</value>
    public object? Body { get; set; }

    /// <summary>
    /// Gets or sets the query parameters for the request.
    /// </summary>
    /// <value>A dictionary of query parameter key-value pairs. Can be null if no query parameters are needed.</value>
    public Dictionary<string, string?>? QueryParameters { get; set; }

    /// <summary>
    /// Gets or sets additional headers for the request.
    /// </summary>
    /// <value>A dictionary of header key-value pairs. Can be null if no additional headers are needed.</value>
    public Dictionary<string, string?>? Headers { get; set; }

    /// <summary>
    /// Builds the full URL for the request by combining the base URL, endpoint, and query parameters.
    /// </summary>
    /// <param name="baseUrl">The base URL of the API (e.g., "https://api.max.ru/bot").</param>
    /// <returns>The full URL including query parameters.</returns>
    /// <exception cref="ArgumentException">Thrown when baseUrl or endpoint is invalid.</exception>
    public string BuildUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("BaseUrl cannot be null or empty.", nameof(baseUrl));
        }

        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(Endpoint));
        }

        var baseUri = baseUrl.TrimEnd('/');
        var endpoint = Endpoint.TrimStart('/');

        var url = $"{baseUri}/{endpoint}";

        var queryString = BuildQueryString();
        if (!string.IsNullOrEmpty(queryString))
        {
            url += $"?{queryString}";
        }

        return url;
    }

    /// <summary>
    /// Builds the query string from the QueryParameters dictionary.
    /// </summary>
    /// <returns>The query string (without the leading "?"), or an empty string if there are no query parameters.</returns>
    public string BuildQueryString()
    {
        if (QueryParameters == null || QueryParameters.Count == 0)
        {
            return string.Empty;
        }

        var queryParts = new List<string>();

        foreach (var kvp in QueryParameters)
        {
            if (kvp.Value == null)
            {
                continue;
            }

            var key = Uri.EscapeDataString(kvp.Key);
            var value = Uri.EscapeDataString(kvp.Value);
            queryParts.Add($"{key}={value}");
        }

        return string.Join("&", queryParts);
    }
}


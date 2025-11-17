namespace Max.Bot.Networking;

/// <summary>
/// Interface for HTTP client that handles communication with the Max Bot API.
/// </summary>
public interface IMaxHttpClient
{
    /// <summary>
    /// Sends an HTTP request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response to.</typeparam>
    /// <param name="request">The API request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized response.</returns>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxRateLimitException">Thrown when rate limit is exceeded.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication or authorization fails.</exception>
    Task<TResponse> SendAsync<TResponse>(MaxApiRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request without expecting a response body.
    /// </summary>
    /// <param name="request">The API request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxRateLimitException">Thrown when rate limit is exceeded.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication or authorization fails.</exception>
    Task SendAsync(MaxApiRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request and returns the raw response body as a string.
    /// This allows trying multiple deserialization strategies on the same response without making multiple HTTP calls.
    /// </summary>
    /// <param name="request">The API request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the raw response body.</returns>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxRateLimitException">Thrown when rate limit is exceeded.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication or authorization fails.</exception>
    Task<string> SendAsyncRaw(MaxApiRequest request, CancellationToken cancellationToken = default);
}


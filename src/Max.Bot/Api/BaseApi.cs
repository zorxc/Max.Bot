using System.Net;
using System.Net.Http;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Networking;
using Max.Bot.Types;

namespace Max.Bot.Api;

/// <summary>
/// Base class for API classes that provides common functionality.
/// </summary>
internal abstract class BaseApi
{
    /// <summary>
    /// Gets the HTTP client for making API requests.
    /// </summary>
    protected readonly IMaxHttpClient HttpClient;

    /// <summary>
    /// Gets the bot options containing token and base URL.
    /// </summary>
    protected readonly MaxBotOptions Options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">The bot options containing token and base URL.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    protected BaseApi(IMaxHttpClient httpClient, MaxBotOptions options)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Builds the endpoint URL path.
    /// </summary>
    /// <param name="path">The endpoint path (e.g., "/me", "/messages").</param>
    /// <returns>The normalized endpoint path (e.g., "/me", "/messages").</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
    protected string BuildEndpoint(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        // Ensure path starts with '/'
        // Token is passed via Authorization header, not in URL path
        return path.StartsWith('/') ? path : $"/{path}";
    }

    /// <summary>
    /// Creates a MaxApiRequest with Authorization header and proper endpoint.
    /// </summary>
    /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
    /// <param name="endpoint">The endpoint path relative to base URL (without token).</param>
    /// <param name="body">The request body object to be serialized to JSON. Can be null.</param>
    /// <param name="queryParams">The query parameters dictionary. Can be null.</param>
    /// <returns>A MaxApiRequest ready to be sent.</returns>
    protected MaxApiRequest CreateRequest(
        HttpMethod method,
        string endpoint,
        object? body = null,
        Dictionary<string, string?>? queryParams = null)
    {
        var request = new MaxApiRequest
        {
            Method = method,
            Endpoint = BuildEndpoint(endpoint),
            Body = body,
            QueryParameters = queryParams
        };

        // Add Authorization header with token (format: "Authorization: <token>" as per Max API docs)
        request.Headers ??= new Dictionary<string, string?>();
        request.Headers["Authorization"] = Options.Token;

        return request;
    }

    /// <summary>
    /// Executes a request and handles Response&lt;T&gt; wrapper.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="request">The API request to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unwrapped response data.</returns>
    /// <exception cref="MaxApiException">Thrown when the API returns an error response (ok: false or result is null).</exception>
    /// <exception cref="MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="MaxUnauthorizedException">Thrown when authentication fails.</exception>
    protected async Task<T> ExecuteRequestAsync<T>(
        MaxApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Special case: if T is Response, deserialize directly without Response<T> wrapper
        if (typeof(T) == typeof(Response) || typeof(T).IsAssignableFrom(typeof(Response)))
        {
            var simpleResponse = await HttpClient.SendAsync<Response>(request, cancellationToken).ConfigureAwait(false);
            if (simpleResponse == null)
            {
                throw new MaxApiException(
                    "API request returned null response.",
                    null,
                    HttpStatusCode.BadRequest);
            }
            return (T)(object)simpleResponse;
        }

        // * Make ONE HTTP request and try different deserialization strategies
        // Some endpoints (like /me) return data directly, others return Response<T>
        // IMPORTANT: For POST/PUT/PATCH requests, we should only make ONE HTTP request to avoid duplicate actions
        // For GET requests, we can try both deserialization strategies if needed
        
        try
        {
            // First try: deserialize as Response<T>
            var wrappedResponse = await HttpClient.SendAsync<Response<T>>(request, cancellationToken).ConfigureAwait(false);
            if (wrappedResponse.Ok && wrappedResponse.Result != null)
            {
                return wrappedResponse.Result;
            }
            
            // * If Response<T> deserialized but ok=false or result=null, this could mean:
            // 1. API error - format is correct but request failed (for POST/PUT/PATCH - throw exception, don't retry)
            // 2. Format mismatch - endpoint returns data directly (for GET - try direct deserialization)
            
            // * For GET requests only: try direct deserialization if Response<T> format doesn't match
            // For POST/PUT/PATCH: if Response<T> deserialized successfully (even with ok=false), 
            // this means the format is correct and it's an API error, not a format mismatch
            // We should NOT make a second request to avoid duplicate actions
            if (request.Method == HttpMethod.Get)
            {
                // * For GET requests: try direct deserialization as T (for endpoints like /me that return data directly)
                try
                {
                    var directResponse = await HttpClient.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);
                    if (directResponse != null)
                    {
                        return directResponse;
                    }
                }
                catch
                {
                    // If direct deserialization also fails, throw original error
                }
            }
            
            // If we get here, either it's a POST/PUT/PATCH with API error, or both deserializations failed for GET
            throw new MaxApiException(
                $"API request returned unsuccessful response. Ok={wrappedResponse.Ok}, Result is null.",
                null,
                HttpStatusCode.BadRequest);
        }
        catch (Exceptions.MaxNetworkException ex) when (ex.Message.Contains("Failed to deserialize") && request.Method == HttpMethod.Get)
        {
            // * For GET requests only: if deserialization as Response<T> failed due to format mismatch,
            // try direct deserialization as T (for endpoints like /me that return data directly)
            // We only do this for GET requests to avoid duplicate actions on POST/PUT/PATCH
            try
            {
                var directResponse = await HttpClient.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);
                if (directResponse != null)
                {
                    return directResponse;
                }
            }
            catch
            {
                // If direct deserialization also fails, rethrow original exception
                throw new MaxApiException(
                    "API request failed. The response could not be deserialized as Response<T> or T.",
                    null,
                    HttpStatusCode.BadRequest);
            }
            
            // If we get here, directResponse was null
            throw new MaxApiException(
                "API request failed. The response could not be deserialized as Response<T> or T.",
                null,
                HttpStatusCode.BadRequest);
        }
        catch (Exceptions.MaxApiException)
        {
            // Re-throw API exceptions as-is
            throw;
        }
    }

    /// <summary>
    /// Validates that a chat ID is greater than zero.
    /// </summary>
    /// <param name="chatId">The chat ID to validate.</param>
    /// <param name="paramName">The name of the parameter (default: "chatId").</param>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    protected static void ValidateChatId(long chatId, string paramName = "chatId")
    {
        if (chatId <= 0)
        {
            throw new ArgumentException("Chat ID must be greater than zero.", paramName);
        }
    }

    /// <summary>
    /// Validates that a user ID is greater than zero.
    /// </summary>
    /// <param name="userId">The user ID to validate.</param>
    /// <param name="paramName">The name of the parameter (default: "userId").</param>
    /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
    protected static void ValidateUserId(long userId, string paramName = "userId")
    {
        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be greater than zero.", paramName);
        }
    }

    /// <summary>
    /// Validates that a string parameter is not null or empty.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
    protected static void ValidateNotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
        }
    }
}


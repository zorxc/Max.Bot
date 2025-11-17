using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Types;
using Microsoft.Extensions.Logging;

namespace Max.Bot.Networking;

/// <summary>
/// HTTP client implementation for communicating with the Max Bot API.
/// </summary>
public class MaxHttpClient : IMaxHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly MaxBotClientOptions _options;
    private readonly ILogger<MaxHttpClient>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The underlying HTTP client to use for requests.</param>
    /// <param name="options">The configuration options for the HTTP client.</param>
    /// <param name="logger">The logger to use for logging events.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public MaxHttpClient(HttpClient httpClient, MaxBotClientOptions options, ILogger<MaxHttpClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        _options.Validate();

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = _options.Timeout;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(
        MaxApiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attempt = 0;
        Exception? lastException = null;
        var stopwatch = Stopwatch.StartNew();

        while (attempt <= _options.RetryCount)
        {
            try
            {
                var url = request.BuildUrl(_options.BaseUrl);
                _logger?.LogDebug(
                    "Sending {Method} request to {Url} (attempt {Attempt}/{TotalAttempts})",
                    request.Method,
                    url,
                    attempt + 1,
                    _options.RetryCount + 1);

                // Log request body if detailed logging is enabled (before creating HttpRequestMessage)
                if (_options.EnableDetailedLogging && request.Body != null)
                {
                    var requestBodyJson = MaxJsonSerializer.Serialize(request.Body);
                    _logger?.LogTrace("Request body: {RequestBody}", requestBodyJson);
                }

                using var httpRequest = await BuildHttpRequestMessageAsync(request, cancellationToken).ConfigureAwait(false);

                var requestStopwatch = Stopwatch.StartNew();
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                requestStopwatch.Stop();

                // Read response content once at the beginning to avoid ObjectDisposedException
                string? responseBody = null;
                if (response.Content != null)
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        throw new MaxNetworkException("Response content was disposed before reading.", ex);
                    }
                }

                // Log response body if detailed logging is enabled
                if (_options.EnableDetailedLogging && responseBody != null)
                {
                    _logger?.LogTrace("Response body: {ResponseBody}", responseBody);
                }

                // Handle response (check status code and throw exceptions if needed)
                await HandleHttpResponseAsync(response, responseBody, cancellationToken).ConfigureAwait(false);

                if (responseBody == null)
                {
                    throw new MaxNetworkException("Response content is null.");
                }

                // Use the already read body for deserialization
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody));
                var responseStream = memoryStream;

                TResponse result;
                try
                {
                    result = MaxJsonSerializer.Deserialize<TResponse>(responseStream);
                }
                catch (Exception deserializeEx)
                {
                    _logger?.LogError(deserializeEx, "Failed to deserialize response. Response body: {ResponseBody}", responseBody);
                    throw new MaxNetworkException($"Failed to deserialize API response: {deserializeEx.Message}. Response body: {responseBody}", deserializeEx);
                }

                stopwatch.Stop();
                _logger?.LogDebug(
                    "Request succeeded with status {StatusCode} in {ElapsedMs}ms",
                    (int)response.StatusCode,
                    requestStopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (ObjectDisposedException ex)
            {
                // ObjectDisposedException when reading content usually means the response was already consumed
                // This can happen in tests with mocked handlers that reuse the same HttpResponseMessage
                // Don't retry in this case - throw immediately
                throw new MaxNetworkException("Response content was disposed unexpectedly. This may indicate a problem with the HTTP handler or test setup.", ex);
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, request) || attempt == _options.RetryCount)
            {
                lastException = ex;
                attempt++;

                _logger?.LogWarning(
                    ex,
                    "Request failed (attempt {Attempt}/{TotalAttempts}): {ErrorMessage}",
                    attempt,
                    _options.RetryCount + 1,
                    ex.Message);

                if (attempt > _options.RetryCount)
                {
                    break;
                }

                var delay = CalculateRetryDelay(ex, attempt);
                _logger?.LogInformation(
                    "Retrying request after {DelayMs}ms (attempt {Attempt}/{TotalAttempts})",
                    delay.TotalMilliseconds,
                    attempt + 1,
                    _options.RetryCount + 1);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        stopwatch.Stop();

        // If we get here, all retries failed
        if (lastException != null)
        {
            _logger?.LogError(
                lastException,
                "All {RetryCount} retry attempts failed after {TotalElapsedMs}ms",
                _options.RetryCount,
                stopwatch.ElapsedMilliseconds);
        }

        throw lastException ?? new MaxNetworkException("Request failed after all retry attempts.");
    }

    /// <inheritdoc />
    public async Task<string> SendAsyncRaw(
        MaxApiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attempt = 0;
        Exception? lastException = null;
        var stopwatch = Stopwatch.StartNew();

        while (attempt <= _options.RetryCount)
        {
            try
            {
                var url = request.BuildUrl(_options.BaseUrl);
                _logger?.LogDebug(
                    "Sending {Method} request to {Url} (attempt {Attempt}/{TotalAttempts})",
                    request.Method,
                    url,
                    attempt + 1,
                    _options.RetryCount + 1);

                // Log request body if detailed logging is enabled
                if (_options.EnableDetailedLogging && request.Body != null)
                {
                    var requestBodyJson = MaxJsonSerializer.Serialize(request.Body);
                    _logger?.LogTrace("Request body: {RequestBody}", requestBodyJson);
                }

                using var httpRequest = await BuildHttpRequestMessageAsync(request, cancellationToken).ConfigureAwait(false);

                var requestStopwatch = Stopwatch.StartNew();
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                requestStopwatch.Stop();

                // Read response content once
                string? responseBody = null;
                if (response.Content != null)
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        throw new MaxNetworkException("Response content was disposed before reading.", ex);
                    }
                }

                // Log response body if detailed logging is enabled
                if (_options.EnableDetailedLogging && responseBody != null)
                {
                    _logger?.LogTrace("Response body: {ResponseBody}", responseBody);
                }

                // Handle response (check status code and throw exceptions if needed)
                await HandleHttpResponseAsync(response, responseBody, cancellationToken).ConfigureAwait(false);

                if (responseBody == null)
                {
                    throw new MaxNetworkException("Response content is null.");
                }

                stopwatch.Stop();
                _logger?.LogDebug(
                    "Request succeeded with status {StatusCode} in {ElapsedMs}ms",
                    (int)response.StatusCode,
                    requestStopwatch.ElapsedMilliseconds);

                return responseBody;
            }
            catch (ObjectDisposedException ex)
            {
                throw new MaxNetworkException("Response content was disposed unexpectedly. This may indicate a problem with the HTTP handler or test setup.", ex);
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, request) || attempt == _options.RetryCount)
            {
                lastException = ex;
                attempt++;

                _logger?.LogWarning(
                    ex,
                    "Request failed (attempt {Attempt}/{TotalAttempts}): {ErrorMessage}",
                    attempt,
                    _options.RetryCount + 1,
                    ex.Message);

                if (attempt > _options.RetryCount)
                {
                    break;
                }

                var delay = CalculateRetryDelay(ex, attempt);
                _logger?.LogInformation(
                    "Retrying request after {DelayMs}ms (attempt {Attempt}/{TotalAttempts})",
                    delay.TotalMilliseconds,
                    attempt + 1,
                    _options.RetryCount + 1);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        stopwatch.Stop();

        if (lastException != null)
        {
            _logger?.LogError(
                lastException,
                "All {RetryCount} retry attempts failed after {TotalElapsedMs}ms",
                _options.RetryCount,
                stopwatch.ElapsedMilliseconds);
        }

        throw lastException ?? new MaxNetworkException("Request failed after all retry attempts.");
    }

    /// <inheritdoc />
    public async Task SendAsync(
        MaxApiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attempt = 0;
        Exception? lastException = null;
        var stopwatch = Stopwatch.StartNew();

        while (attempt <= _options.RetryCount)
        {
            try
            {
                var url = request.BuildUrl(_options.BaseUrl);
                _logger?.LogDebug(
                    "Sending {Method} request to {Url} (attempt {Attempt}/{TotalAttempts})",
                    request.Method,
                    url,
                    attempt + 1,
                    _options.RetryCount + 1);

                // Log request body if detailed logging is enabled (before creating HttpRequestMessage)
                if (_options.EnableDetailedLogging && request.Body != null)
                {
                    var requestBodyJson = MaxJsonSerializer.Serialize(request.Body);
                    _logger?.LogTrace("Request body: {RequestBody}", requestBodyJson);
                }

                using var httpRequest = await BuildHttpRequestMessageAsync(request, cancellationToken).ConfigureAwait(false);

                var requestStopwatch = Stopwatch.StartNew();
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                requestStopwatch.Stop();

                // Read response content once at the beginning to avoid ObjectDisposedException
                string? responseBody = null;
                if (response.Content != null)
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        throw new MaxNetworkException("Response content was disposed before reading.", ex);
                    }
                }

                // Log response body if detailed logging is enabled
                if (_options.EnableDetailedLogging && responseBody != null)
                {
                    _logger?.LogTrace("Response body: {ResponseBody}", responseBody);
                }

                // Handle response (check status code and throw exceptions if needed)
                await HandleHttpResponseAsync(response, responseBody, cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                _logger?.LogDebug(
                    "Request succeeded with status {StatusCode} in {ElapsedMs}ms",
                    (int)response.StatusCode,
                    requestStopwatch.ElapsedMilliseconds);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                // ObjectDisposedException when reading content usually means the response was already consumed
                // This can happen in tests with mocked handlers that reuse the same HttpResponseMessage
                // Don't retry in this case - throw immediately
                throw new MaxNetworkException("Response content was disposed unexpectedly. This may indicate a problem with the HTTP handler or test setup.", ex);
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, request) || attempt == _options.RetryCount)
            {
                lastException = ex;
                attempt++;

                _logger?.LogWarning(
                    ex,
                    "Request failed (attempt {Attempt}/{TotalAttempts}): {ErrorMessage}",
                    attempt,
                    _options.RetryCount + 1,
                    ex.Message);

                if (attempt > _options.RetryCount)
                {
                    break;
                }

                var delay = CalculateRetryDelay(ex, attempt);
                _logger?.LogInformation(
                    "Retrying request after {DelayMs}ms (attempt {Attempt}/{TotalAttempts})",
                    delay.TotalMilliseconds,
                    attempt + 1,
                    _options.RetryCount + 1);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        stopwatch.Stop();

        // If we get here, all retries failed
        if (lastException != null)
        {
            _logger?.LogError(
                lastException,
                "All {RetryCount} retry attempts failed after {TotalElapsedMs}ms",
                _options.RetryCount,
                stopwatch.ElapsedMilliseconds);
        }

        throw lastException ?? new MaxNetworkException("Request failed after all retry attempts.");
    }

    /// <summary>
    /// Builds an HttpRequestMessage from a MaxApiRequest.
    /// </summary>
    /// <param name="request">The API request to convert.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An HttpRequestMessage ready to be sent.</returns>
    private Task<HttpRequestMessage> BuildHttpRequestMessageAsync(
        MaxApiRequest request,
        CancellationToken cancellationToken)
    {
        var url = request.BuildUrl(_options.BaseUrl);
        var httpRequest = new HttpRequestMessage(request.Method, url);

        // Add headers
        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                if (string.IsNullOrEmpty(header.Value))
                {
                    continue;
                }

                // Skip content-type header - it will be set when adding body
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // * Authorization header - Max API expects "Authorization: <token>" (not Bearer)
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    // * Max API expects just the token, not "Bearer <token>"
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", header.Value);
                }
                else
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Add body if present (POST, PUT, PATCH, DELETE can have JSON body)
        if (request.Body != null && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put || request.Method == HttpMethod.Patch || request.Method == HttpMethod.Delete))
        {
            var json = MaxJsonSerializer.Serialize(request.Body);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return Task.FromResult(httpRequest);
    }

    /// <summary>
    /// Handles the HTTP response and throws appropriate exceptions if the response indicates an error.
    /// </summary>
    /// <param name="response">The HTTP response to handle.</param>
    /// <param name="responseBody">The pre-read response body content, or null if not available.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    private Task HandleHttpResponseAsync(
        HttpResponseMessage response,
        string? responseBody,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return Task.CompletedTask;
        }

        // Log error response if detailed logging is enabled
        if (_options.EnableDetailedLogging)
        {
            _logger?.LogError("Error Response Status: {StatusCode} {StatusReason}", (int)response.StatusCode, response.StatusCode);
            if (responseBody != null)
            {
                _logger?.LogError("Error Response Body: {ResponseBody}", responseBody);
            }
        }

        string? errorMessage = null;
        string? errorCode = null;

        // Try to deserialize as ErrorResponse if response body is available
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                var errorResponse = MaxJsonSerializer.Deserialize<ErrorResponse>(responseBody);
                if (errorResponse?.Error != null)
                {
                    errorMessage = errorResponse.Error.Message ?? responseBody;
                    errorCode = errorResponse.Error.Code;
                }
                else
                {
                    // If deserialization succeeded but Error is null, use raw body
                    errorMessage = responseBody;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // If deserialization fails, use raw response body as error message
                errorMessage = responseBody;
            }
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
        }

        // Map HTTP status codes to specific exceptions
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new MaxUnauthorizedException(
                errorMessage,
                errorCode,
                response.StatusCode),
            HttpStatusCode.TooManyRequests => new MaxRateLimitException(
                errorMessage,
                errorCode,
                response.StatusCode,
                GetRetryAfter(response)),
            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => new MaxNetworkException(
                errorMessage,
                errorCode,
                response.StatusCode),
            _ when (int)response.StatusCode >= 500 => new MaxNetworkException(
                errorMessage,
                errorCode,
                response.StatusCode),
            _ => new MaxApiException(
                errorMessage,
                errorCode,
                response.StatusCode),
        };
    }

    /// <summary>
    /// Gets the Retry-After value from the response headers.
    /// </summary>
    /// <param name="response">The HTTP response to check.</param>
    /// <returns>The Retry-After timespan, or null if not present.</returns>
    private TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            return null;
        }

        var retryAfterValue = retryAfterValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(retryAfterValue))
        {
            return null;
        }

        // Retry-After can be either a number of seconds or an HTTP date
        if (int.TryParse(retryAfterValue, out var seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }

        if (DateTimeOffset.TryParse(retryAfterValue, out var date))
        {
            return date - DateTimeOffset.UtcNow;
        }

        return null;
    }

    /// <summary>
    /// Determines whether an exception should trigger a retry.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="attempt">The current attempt number (0-based).</param>
    /// <param name="request">The API request that failed (optional, used to detect long polling requests).</param>
    /// <returns>True if the request should be retried; otherwise, false.</returns>
    private bool ShouldRetry(Exception exception, int attempt, MaxApiRequest? request = null)
    {
        // Don't retry if we've exceeded the retry count
        if (attempt >= _options.RetryCount)
        {
            return false;
        }

        // * For long polling requests (/updates), timeouts are expected behavior when there are no updates
        // The server keeps the connection open until timeout, so HttpClient timeouts should not trigger retries
        // This prevents unnecessary retry attempts and log noise
        var isLongPollingRequest = request != null &&
            request.Method == HttpMethod.Get &&
            request.Endpoint.Equals("/updates", StringComparison.OrdinalIgnoreCase);

        // Retry on network exceptions (timeouts, connection errors)
        // But skip retry for long polling timeouts - they're expected when no updates are available
        if (exception is TaskCanceledException && isLongPollingRequest)
        {
            // For long polling, TaskCanceledException due to timeout is expected - don't retry
            // The UpdatePoller will make a new request anyway
            return false;
        }

        if (exception is MaxNetworkException || exception is HttpRequestException || exception is TaskCanceledException)
        {
            return true;
        }

        // Retry on rate limit exceptions
        if (exception is MaxRateLimitException)
        {
            return true;
        }

        // Don't retry on authentication/authorization errors
        if (exception is MaxUnauthorizedException)
        {
            return false;
        }

        // Don't retry on client errors (4xx) except rate limiting
        if (exception is MaxApiException apiEx && apiEx.HttpStatusCode.HasValue)
        {
            var statusCode = apiEx.HttpStatusCode.Value;
            // Retry on 5xx errors (server errors)
            if ((int)statusCode >= 500)
            {
                return true;
            }

            // Retry on timeout errors
            if (statusCode == HttpStatusCode.RequestTimeout || statusCode == HttpStatusCode.GatewayTimeout)
            {
                return true;
            }

            // Don't retry on other 4xx errors
            return false;
        }

        // Default: don't retry
        return false;
    }

    /// <summary>
    /// Calculates the delay before retrying based on the exception and attempt number.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="attempt">The current attempt number (1-based).</param>
    /// <returns>The delay to wait before retrying.</returns>
    private TimeSpan CalculateRetryDelay(Exception exception, int attempt)
    {
        // For rate limit exceptions, use Retry-After if available
        if (exception is MaxRateLimitException rateLimitEx && rateLimitEx.RetryAfter.HasValue)
        {
            var retryAfter = rateLimitEx.RetryAfter.Value;
            // Ensure it doesn't exceed MaxRetryDelay
            return retryAfter > _options.MaxRetryDelay ? _options.MaxRetryDelay : retryAfter;
        }

        // Calculate exponential backoff: baseDelay * 2^(attempt-1)
        var delay = TimeSpan.FromMilliseconds(
            _options.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));

        // Add jitter to prevent thundering herd (random factor between 0.8 and 1.2)
        var random = new Random();
        var jitter = 0.8 + (random.NextDouble() * 0.4); // 0.8 to 1.2
        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitter);

        // Ensure it doesn't exceed MaxRetryDelay
        if (delay > _options.MaxRetryDelay)
        {
            delay = _options.MaxRetryDelay;
        }

        return delay;
    }
}



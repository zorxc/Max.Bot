using System.Net;

namespace Max.Bot.Exceptions;

/// <summary>
/// Exception thrown when the rate limit is exceeded (HTTP 429).
/// </summary>
public class MaxRateLimitException : MaxApiException
{
    /// <summary>
    /// Gets the time after which the request can be retried, if specified in the Retry-After header.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxRateLimitException"/> class.
    /// </summary>
    public MaxRateLimitException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxRateLimitException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MaxRateLimitException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxRateLimitException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MaxRateLimitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxRateLimitException"/> class with a specified error message, error code, HTTP status code, and retry after time.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code from the API response.</param>
    /// <param name="httpStatusCode">The HTTP status code from the API response.</param>
    /// <param name="retryAfter">The time after which the request can be retried.</param>
    public MaxRateLimitException(string message, string? errorCode, HttpStatusCode? httpStatusCode, TimeSpan? retryAfter)
        : base(message, errorCode, httpStatusCode)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxRateLimitException"/> class with a specified error message, error code, HTTP status code, retry after time, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code from the API response.</param>
    /// <param name="httpStatusCode">The HTTP status code from the API response.</param>
    /// <param name="retryAfter">The time after which the request can be retried.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MaxRateLimitException(string message, string? errorCode, HttpStatusCode? httpStatusCode, TimeSpan? retryAfter, Exception innerException)
        : base(message, errorCode, httpStatusCode, innerException)
    {
        RetryAfter = retryAfter;
    }
}


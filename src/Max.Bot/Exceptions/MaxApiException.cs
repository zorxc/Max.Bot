using System.Net;

namespace Max.Bot.Exceptions;

/// <summary>
/// Base exception for all Max Bot API errors.
/// </summary>
public class MaxApiException : Exception
{
    /// <summary>
    /// Gets the error code from the API response, if available.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the HTTP status code from the API response.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxApiException"/> class.
    /// </summary>
    public MaxApiException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxApiException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MaxApiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxApiException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MaxApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxApiException"/> class with a specified error message, error code, and HTTP status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code from the API response.</param>
    /// <param name="httpStatusCode">The HTTP status code from the API response.</param>
    public MaxApiException(string message, string? errorCode, HttpStatusCode? httpStatusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxApiException"/> class with a specified error message, error code, HTTP status code, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code from the API response.</param>
    /// <param name="httpStatusCode">The HTTP status code from the API response.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MaxApiException(string message, string? errorCode, HttpStatusCode? httpStatusCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }
}


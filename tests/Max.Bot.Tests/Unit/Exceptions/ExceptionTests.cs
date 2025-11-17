using System.Net;
using FluentAssertions;
using Max.Bot.Exceptions;
using Xunit;

namespace Max.Bot.Tests.Unit.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void MaxApiException_ShouldCreate_WithDefaultConstructor()
    {
        // Act
        var exception = new MaxApiException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeEmpty(); // .NET sets default message
        exception.ErrorCode.Should().BeNull();
        exception.HttpStatusCode.Should().BeNull();
    }

    [Fact]
    public void MaxApiException_ShouldCreate_WithMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new MaxApiException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().BeNull();
        exception.HttpStatusCode.Should().BeNull();
    }

    [Fact]
    public void MaxApiException_ShouldCreate_WithMessageAndInnerException()
    {
        // Arrange
        var message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new MaxApiException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.ErrorCode.Should().BeNull();
        exception.HttpStatusCode.Should().BeNull();
    }

    [Fact]
    public void MaxApiException_ShouldCreate_WithMessageErrorCodeAndHttpStatusCode()
    {
        // Arrange
        var message = "Test error message";
        var errorCode = "ERROR_CODE";
        var httpStatusCode = HttpStatusCode.BadRequest;

        // Act
        var exception = new MaxApiException(message, errorCode, httpStatusCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
    }

    [Fact]
    public void MaxApiException_ShouldCreate_WithAllParameters()
    {
        // Arrange
        var message = "Test error message";
        var errorCode = "ERROR_CODE";
        var httpStatusCode = HttpStatusCode.BadRequest;
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new MaxApiException(message, errorCode, httpStatusCode, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void MaxNetworkException_ShouldInheritFromMaxApiException()
    {
        // Act
        var exception = new MaxNetworkException();

        // Assert
        exception.Should().BeAssignableTo<MaxApiException>();
    }

    [Fact]
    public void MaxNetworkException_ShouldCreate_WithMessage()
    {
        // Arrange
        var message = "Network error";

        // Act
        var exception = new MaxNetworkException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void MaxNetworkException_ShouldCreate_WithMessageAndInnerException()
    {
        // Arrange
        var message = "Network error";
        var innerException = new TimeoutException("Timeout");

        // Act
        var exception = new MaxNetworkException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void MaxNetworkException_ShouldCreate_WithMessageErrorCodeAndHttpStatusCode()
    {
        // Arrange
        var message = "Network error";
        var errorCode = "NETWORK_ERROR";
        var httpStatusCode = HttpStatusCode.InternalServerError;

        // Act
        var exception = new MaxNetworkException(message, errorCode, httpStatusCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
    }

    [Fact]
    public void MaxRateLimitException_ShouldInheritFromMaxApiException()
    {
        // Act
        var exception = new MaxRateLimitException();

        // Assert
        exception.Should().BeAssignableTo<MaxApiException>();
    }

    [Fact]
    public void MaxRateLimitException_ShouldCreate_WithMessage()
    {
        // Arrange
        var message = "Rate limit exceeded";

        // Act
        var exception = new MaxRateLimitException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.RetryAfter.Should().BeNull();
    }

    [Fact]
    public void MaxRateLimitException_ShouldCreate_WithRetryAfter()
    {
        // Arrange
        var message = "Rate limit exceeded";
        var errorCode = "RATE_LIMIT";
        var httpStatusCode = HttpStatusCode.TooManyRequests;
        var retryAfter = TimeSpan.FromSeconds(60);

        // Act
        var exception = new MaxRateLimitException(message, errorCode, httpStatusCode, retryAfter);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
        exception.RetryAfter.Should().Be(retryAfter);
    }

    [Fact]
    public void MaxRateLimitException_ShouldCreate_WithAllParameters()
    {
        // Arrange
        var message = "Rate limit exceeded";
        var errorCode = "RATE_LIMIT";
        var httpStatusCode = HttpStatusCode.TooManyRequests;
        var retryAfter = TimeSpan.FromSeconds(60);
        var innerException = new HttpRequestException("Request failed");

        // Act
        var exception = new MaxRateLimitException(message, errorCode, httpStatusCode, retryAfter, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
        exception.RetryAfter.Should().Be(retryAfter);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void MaxUnauthorizedException_ShouldInheritFromMaxApiException()
    {
        // Act
        var exception = new MaxUnauthorizedException();

        // Assert
        exception.Should().BeAssignableTo<MaxApiException>();
    }

    [Fact]
    public void MaxUnauthorizedException_ShouldCreate_WithMessage()
    {
        // Arrange
        var message = "Unauthorized";

        // Act
        var exception = new MaxUnauthorizedException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void MaxUnauthorizedException_ShouldCreate_WithHttp401StatusCode()
    {
        // Arrange
        var message = "Unauthorized";
        var errorCode = "UNAUTHORIZED";
        var httpStatusCode = HttpStatusCode.Unauthorized;

        // Act
        var exception = new MaxUnauthorizedException(message, errorCode, httpStatusCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
    }

    [Fact]
    public void MaxUnauthorizedException_ShouldCreate_WithHttp403StatusCode()
    {
        // Arrange
        var message = "Forbidden";
        var errorCode = "FORBIDDEN";
        var httpStatusCode = HttpStatusCode.Forbidden;

        // Act
        var exception = new MaxUnauthorizedException(message, errorCode, httpStatusCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be(errorCode);
        exception.HttpStatusCode.Should().Be(httpStatusCode);
    }

    [Fact]
    public void Exception_ShouldSupportPolymorphism()
    {
        // Arrange
        MaxApiException baseException = new MaxNetworkException("Network error");
        MaxApiException rateLimitException = new MaxRateLimitException("Rate limit exceeded");
        MaxApiException unauthorizedException = new MaxUnauthorizedException("Unauthorized");

        // Act & Assert
        baseException.Should().BeOfType<MaxNetworkException>();
        rateLimitException.Should().BeOfType<MaxRateLimitException>();
        unauthorizedException.Should().BeOfType<MaxUnauthorizedException>();
    }
}


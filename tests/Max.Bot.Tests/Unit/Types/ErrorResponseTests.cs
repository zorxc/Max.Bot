using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class ErrorResponseTests
{
    [Fact]
    public void Deserialize_ShouldDeserializeErrorResponse()
    {
        // Arrange
        var json = """{"ok":false,"error":{"code":"ERROR_CODE","message":"Error message"}}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<ErrorResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Ok.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ERROR_CODE");
        result.Error.Message.Should().Be("Error message");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeErrorResponseWithNullError()
    {
        // Arrange
        var json = """{"ok":false,"error":null}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<ErrorResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Ok.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Serialize_ShouldSerializeErrorResponse()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Ok = false,
            Error = new Error
            {
                Code = "ERROR_CODE",
                Message = "Error message"
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize(errorResponse);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"ok\":false");
        json.Should().Contain("\"error\"");
        json.Should().Contain("\"code\":\"ERROR_CODE\"");
        json.Should().Contain("\"message\":\"Error message\"");
    }

    [Fact]
    public void Serialize_ShouldNotIncludeNullError()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Ok = false,
            Error = null
        };

        // Act
        var json = MaxJsonSerializer.Serialize(errorResponse);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"ok\":false");
        json.Should().NotContain("\"error\"");
    }
}


using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class ErrorTests
{
    [Fact]
    public void Deserialize_ShouldDeserializeError()
    {
        // Arrange
        var json = """{"code":"ERROR_CODE","message":"Error message","details":{"field":"value"}}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Error>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("ERROR_CODE");
        result.Message.Should().Be("Error message");
        result.Details.Should().NotBeNull();
        result.Details!["field"].Should().NotBeNull();
    }

    [Fact]
    public void Deserialize_ShouldDeserializeErrorWithoutDetails()
    {
        // Arrange
        var json = """{"code":"ERROR_CODE","message":"Error message"}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<Error>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("ERROR_CODE");
        result.Message.Should().Be("Error message");
        result.Details.Should().BeNull();
    }

    [Fact]
    public void Serialize_ShouldSerializeError()
    {
        // Arrange
        var error = new Error
        {
            Code = "ERROR_CODE",
            Message = "Error message",
            Details = new Dictionary<string, object> { { "field", "value" } }
        };

        // Act
        var json = MaxJsonSerializer.Serialize(error);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"code\":\"ERROR_CODE\"");
        json.Should().Contain("\"message\":\"Error message\"");
        json.Should().Contain("\"details\"");
    }

    [Fact]
    public void Serialize_ShouldNotIncludeNullDetails()
    {
        // Arrange
        var error = new Error
        {
            Code = "ERROR_CODE",
            Message = "Error message",
            Details = null
        };

        // Act
        var json = MaxJsonSerializer.Serialize(error);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"code\":\"ERROR_CODE\"");
        json.Should().Contain("\"message\":\"Error message\"");
        json.Should().NotContain("\"details\"");
    }
}


using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class UserTests
{
    [Fact]
    public void Deserialize_ShouldDeserializeUser()
    {
        // Arrange
        var json = """{"user_id":123,"username":"testuser","first_name":"Test","last_name":"User","is_bot":false}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<User>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Username.Should().Be("testuser");
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("User");
        result.IsBot.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_ShouldDeserializeUserWithNullFields()
    {
        // Arrange
        var json = """{"user_id":123,"is_bot":true}""";

        // Act
        var result = MaxJsonSerializer.Deserialize<User>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Username.Should().BeNull();
        result.FirstName.Should().BeNull();
        result.LastName.Should().BeNull();
        result.IsBot.Should().BeTrue();
    }

    [Fact]
    public void Serialize_ShouldSerializeUser()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            IsBot = false
        };

        // Act
        var json = MaxJsonSerializer.Serialize(user);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"user_id\":123");
        json.Should().Contain("\"username\":\"testuser\"");
        json.Should().Contain("\"first_name\":\"Test\"");
        json.Should().Contain("\"last_name\":\"User\"");
        json.Should().Contain("\"is_bot\":false");
    }

    [Fact]
    public void Serialize_ShouldNotIncludeNullFields()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            IsBot = true
        };

        // Act
        var json = MaxJsonSerializer.Serialize(user);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"user_id\":123");
        json.Should().Contain("\"is_bot\":true");
        json.Should().NotContain("\"username\"");
        json.Should().NotContain("\"firstName\"");
        json.Should().NotContain("\"lastName\"");
    }
}


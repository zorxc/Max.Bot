using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Max.Bot.Networking;
using Xunit;

namespace Max.Bot.Tests.Unit.Networking;

public class MaxJsonSerializerTests
{
    [Fact]
    public void MaxJsonSerializer_Options_ShouldBeConfiguredCorrectly()
    {
        // Act
        var options = MaxJsonSerializer.Options;

        // Assert
        options.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
        options.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.WhenWritingNull);
        options.WriteIndented.Should().BeFalse();
    }

    [Fact]
    public void Serialize_ShouldSerializeSimpleType()
    {
        // Arrange
        var value = "test string";

        // Act
        var json = MaxJsonSerializer.Serialize(value);

        // Assert
        json.Should().Be("\"test string\"");
    }

    [Fact]
    public void Serialize_ShouldSerializeInteger()
    {
        // Arrange
        var value = 42;

        // Act
        var json = MaxJsonSerializer.Serialize(value);

        // Assert
        json.Should().Be("42");
    }

    [Fact]
    public void Serialize_ShouldSerializeComplexObject()
    {
        // Arrange
        var obj = new TestObject
        {
            Id = 1,
            Name = "Test",
            IsActive = true
        };

        // Act
        var json = MaxJsonSerializer.Serialize(obj);

        // Assert
        json.Should().Contain("\"id\":1");
        json.Should().Contain("\"name\":\"Test\"");
        json.Should().Contain("\"isActive\":true");
    }

    [Fact]
    public void Serialize_ShouldUseCamelCase()
    {
        // Arrange
        var obj = new TestObject
        {
            Id = 1,
            Name = "Test",
            IsActive = true
        };

        // Act
        var json = MaxJsonSerializer.Serialize(obj);

        // Assert
        json.Should().Contain("id");
        json.Should().Contain("name");
        json.Should().Contain("isActive");
        json.Should().NotContain("Id");
        json.Should().NotContain("Name");
        json.Should().NotContain("IsActive");
    }

    [Fact]
    public void Serialize_ShouldIgnoreNullValues()
    {
        // Arrange
        var obj = new TestObject
        {
            Id = 1,
            Name = null,
            IsActive = true
        };

        // Act
        var json = MaxJsonSerializer.Serialize(obj);

        // Assert
        json.Should().NotContain("\"name\":");
    }

    [Fact]
    public void Serialize_ShouldThrow_WhenValueIsNull()
    {
        // Arrange
        TestObject? value = null;

        // Act
        var act = () => MaxJsonSerializer.Serialize(value!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(value));
    }

    [Fact]
    public void Deserialize_ShouldDeserializeSimpleType()
    {
        // Arrange
        var json = "\"test string\"";

        // Act
        var result = MaxJsonSerializer.Deserialize<string>(json);

        // Assert
        result.Should().Be("test string");
    }

    [Fact]
    public void Deserialize_ShouldDeserializeInteger()
    {
        // Arrange
        var json = "42";

        // Act
        var result = MaxJsonSerializer.Deserialize<int>(json);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void Deserialize_ShouldDeserializeComplexObject()
    {
        // Arrange
        var json = "{\"id\":1,\"name\":\"Test\",\"isActive\":true}";

        // Act
        var result = MaxJsonSerializer.Deserialize<TestObject>(json);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_ShouldHandleCamelCase()
    {
        // Arrange
        var json = "{\"id\":1,\"name\":\"Test\",\"isActive\":true}";

        // Act
        var result = MaxJsonSerializer.Deserialize<TestObject>(json);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        result.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deserialize_ShouldThrow_WhenJsonIsNullOrEmpty(string? json)
    {
        // Act
        var act = () => MaxJsonSerializer.Deserialize<string>(json!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(json));
    }

    [Fact]
    public void Deserialize_ShouldThrow_WhenJsonIsInvalid()
    {
        // Arrange
        var json = "{invalid json}";

        // Act
        var act = () => MaxJsonSerializer.Deserialize<TestObject>(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_FromStream_ShouldDeserializeObject()
    {
        // Arrange
        var json = "{\"id\":1,\"name\":\"Test\",\"isActive\":true}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var result = MaxJsonSerializer.Deserialize<TestObject>(stream);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_FromStream_ShouldThrow_WhenStreamIsNull()
    {
        // Arrange
        Stream? stream = null;

        // Act
        var act = () => MaxJsonSerializer.Deserialize<TestObject>(stream!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(stream));
    }

    [Fact]
    public void Deserialize_FromStream_ShouldThrow_WhenJsonIsInvalid()
    {
        // Arrange
        var json = "{invalid json}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var act = () => MaxJsonSerializer.Deserialize<TestObject>(stream);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Serialize_And_Deserialize_ShouldBeReversible()
    {
        // Arrange
        var original = new TestObject
        {
            Id = 1,
            Name = "Test",
            IsActive = true
        };

        // Act
        var json = MaxJsonSerializer.Serialize(original);
        var deserialized = MaxJsonSerializer.Deserialize<TestObject>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(original);
    }

    private class TestObject
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool IsActive { get; set; }
    }
}


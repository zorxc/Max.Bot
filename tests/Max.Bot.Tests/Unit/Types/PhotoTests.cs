// 📁 [PhotoTests] - Тесты для модели Photo
// 🎯 Core function: Тестирование сериализации/десериализации Photo
// 🔗 Key dependencies: Max.Bot.Types, Max.Bot.Networking, FluentAssertions, xUnit
// 💡 Usage: Unit тесты для проверки корректности работы модели Photo

using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class PhotoTests
{
    [Fact]
    public void Photo_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"id":123,"fileId":"file123","width":640,"height":480,"fileSize":1024,"url":"https://example.com/photo.jpg"}""";

        // Act
        var photo = MaxJsonSerializer.Deserialize<Photo>(json);

        // Assert
        photo.Should().NotBeNull();
        photo.Id.Should().Be(123);
        photo.FileId.Should().Be("file123");
        photo.Width.Should().Be(640);
        photo.Height.Should().Be(480);
        photo.FileSize.Should().Be(1024);
        photo.Url.Should().Be("https://example.com/photo.jpg");
    }

    [Fact]
    public void Photo_ShouldDeserialize_WithNullableFields()
    {
        // Arrange
        var json = """{"id":123,"fileId":"file123","width":640,"height":480}""";

        // Act
        var photo = MaxJsonSerializer.Deserialize<Photo>(json);

        // Assert
        photo.Should().NotBeNull();
        photo.Id.Should().Be(123);
        photo.FileId.Should().Be("file123");
        photo.Width.Should().Be(640);
        photo.Height.Should().Be(480);
        photo.FileSize.Should().BeNull();
        photo.Url.Should().BeNull();
    }

    [Fact]
    public void Photo_ShouldSerialize_ToJson()
    {
        // Arrange
        var photo = new Photo
        {
            Id = 123,
            FileId = "file123",
            Width = 640,
            Height = 480,
            FileSize = 1024,
            Url = "https://example.com/photo.jpg"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(photo);

        // Assert
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"fileId\":\"file123\"");
        json.Should().Contain("\"width\":640");
        json.Should().Contain("\"height\":480");
        json.Should().Contain("\"fileSize\":1024");
        json.Should().Contain("\"url\":\"https://example.com/photo.jpg\"");
    }

    [Fact]
    public void Photo_ShouldSerialize_WithoutNullableFields()
    {
        // Arrange
        var photo = new Photo
        {
            Id = 123,
            FileId = "file123",
            Width = 640,
            Height = 480
        };

        // Act
        var json = MaxJsonSerializer.Serialize(photo);

        // Assert
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"fileId\":\"file123\"");
        json.Should().Contain("\"width\":640");
        json.Should().Contain("\"height\":480");
        json.Should().NotContain("\"fileSize\"");
        json.Should().NotContain("\"url\"");
    }
}


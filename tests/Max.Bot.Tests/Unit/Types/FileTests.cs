// 📁 [FileTests] - Тесты для модели File
// 🎯 Core function: Тестирование сериализации/десериализации File
// 🔗 Key dependencies: Max.Bot.Types, Max.Bot.Networking, FluentAssertions, xUnit
// 💡 Usage: Unit тесты для проверки корректности работы модели File

using FluentAssertions;
using Max.Bot.Networking;
using MaxBotFile = Max.Bot.Types.File;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class FileTests
{
    [Fact]
    public void File_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"fileId":"file123","fileSize":1024,"filePath":"/path/to/file"}""";

        // Act
        var file = MaxJsonSerializer.Deserialize<MaxBotFile>(json);

        // Assert
        file.Should().NotBeNull();
        file.FileId.Should().Be("file123");
        file.FileSize.Should().Be(1024);
        file.FilePath.Should().Be("/path/to/file");
    }

    [Fact]
    public void File_ShouldDeserialize_WithNullableFields()
    {
        // Arrange
        var json = """{"fileId":"file123"}""";

        // Act
        var file = MaxJsonSerializer.Deserialize<MaxBotFile>(json);

        // Assert
        file.Should().NotBeNull();
        file.FileId.Should().Be("file123");
        file.FileSize.Should().BeNull();
        file.FilePath.Should().BeNull();
    }

    [Fact]
    public void File_ShouldSerialize_ToJson()
    {
        // Arrange
        var file = new MaxBotFile
        {
            FileId = "file123",
            FileSize = 1024,
            FilePath = "/path/to/file"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(file);

        // Assert
        json.Should().Contain("\"fileId\":\"file123\"");
        json.Should().Contain("\"fileSize\":1024");
        json.Should().Contain("\"filePath\":\"/path/to/file\"");
    }
}


using FluentAssertions;
using Max.Bot.Networking;
using Xunit;
using MaxBotFile = Max.Bot.Types.File;

namespace Max.Bot.Tests.Unit.Types;

public class FileTests
{
    [Fact]
    public void File_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"file_id":"file123","file_size":1024,"file_path":"/path/to/file"}""";

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
        var json = """{"file_id":"file123"}""";

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
        json.Should().Contain("\"file_id\":\"file123\"");
        json.Should().Contain("\"file_size\":1024");
        json.Should().Contain("\"file_path\":\"/path/to/file\"");
    }
}


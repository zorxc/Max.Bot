using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class AudioTests
{
    [Fact]
    public void Audio_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"id":123,"file_id":"audio123","duration":180,"file_size":512000,"mime_type":"audio/mpeg","url":"https://example.com/audio.mp3"}""";

        // Act
        var audio = MaxJsonSerializer.Deserialize<Audio>(json);

        // Assert
        audio.Should().NotBeNull();
        audio.Id.Should().Be(123);
        audio.FileId.Should().Be("audio123");
        audio.Duration.Should().Be(180);
        audio.FileSize.Should().Be(512000);
        audio.MimeType.Should().Be("audio/mpeg");
        audio.Url.Should().Be("https://example.com/audio.mp3");
    }

    [Fact]
    public void Audio_ShouldDeserialize_WithNullableFields()
    {
        // Arrange
        var json = """{"id":123,"file_id":"audio123"}""";

        // Act
        var audio = MaxJsonSerializer.Deserialize<Audio>(json);

        // Assert
        audio.Should().NotBeNull();
        audio.Id.Should().Be(123);
        audio.FileId.Should().Be("audio123");
        audio.Duration.Should().BeNull();
        audio.FileSize.Should().BeNull();
        audio.MimeType.Should().BeNull();
        audio.Url.Should().BeNull();
    }

    [Fact]
    public void Audio_ShouldSerialize_ToJson()
    {
        // Arrange
        var audio = new Audio
        {
            Id = 123,
            FileId = "audio123",
            Duration = 180,
            FileSize = 512000,
            MimeType = "audio/mpeg",
            Url = "https://example.com/audio.mp3"
        };

        // Act
        var json = MaxJsonSerializer.Serialize(audio);

        // Assert
        json.Should().Contain("\"id\":123");
        json.Should().Contain("\"file_id\":\"audio123\"");
        json.Should().Contain("\"duration\":180");
        json.Should().Contain("\"file_size\":512000");
        json.Should().Contain("\"mime_type\":\"audio/mpeg\"");
        json.Should().Contain("\"url\":\"https://example.com/audio.mp3\"");
    }
}


using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Xunit;

namespace Max.Bot.Tests.Unit.Types;

public class AttachmentTests
{
    [Fact]
    public void PhotoAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"type":"image","photo":{"id":123,"file_id":"file123","width":640,"height":480}}""";

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<PhotoAttachment>();
        attachment.Type.Should().Be(MessageType.Image);
        var photoAttachment = (PhotoAttachment)attachment;
        photoAttachment.Photo.Should().NotBeNull();
        photoAttachment.Photo.Id.Should().Be(123);
        photoAttachment.Photo.FileId.Should().Be("file123");
        photoAttachment.Photo.Width.Should().Be(640);
        photoAttachment.Photo.Height.Should().Be(480);
    }

    [Fact]
    public void VideoAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"type":"file","video":{"id":123,"file_id":"video123","width":1280,"height":720}}""";

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<VideoAttachment>();
        attachment.Type.Should().Be(MessageType.File);
        var videoAttachment = (VideoAttachment)attachment;
        videoAttachment.Video.Should().NotBeNull();
        videoAttachment.Video.Id.Should().Be(123);
        videoAttachment.Video.FileId.Should().Be("video123");
    }

    [Fact]
    public void AudioAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"type":"file","audio":{"id":123,"file_id":"audio123","duration":180}}""";

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<AudioAttachment>();
        attachment.Type.Should().Be(MessageType.File);
        var audioAttachment = (AudioAttachment)attachment;
        audioAttachment.Audio.Should().NotBeNull();
        audioAttachment.Audio.Id.Should().Be(123);
        audioAttachment.Audio.FileId.Should().Be("audio123");
    }

    [Fact]
    public void DocumentAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"type":"file","document":{"id":123,"file_id":"doc123","file_name":"document.pdf"}}""";

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<DocumentAttachment>();
        attachment.Type.Should().Be(MessageType.File);
        var documentAttachment = (DocumentAttachment)attachment;
        documentAttachment.Document.Should().NotBeNull();
        documentAttachment.Document.Id.Should().Be(123);
        documentAttachment.Document.FileId.Should().Be("doc123");
        documentAttachment.Document.FileName.Should().Be("document.pdf");
    }

    [Fact]
    public void PhotoAttachment_ShouldSerialize_ToJson()
    {
        // Arrange
        var attachment = new PhotoAttachment
        {
            Photo = new Photo
            {
                Id = 123,
                FileId = "file123",
                Width = 640,
                Height = 480
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize<Attachment>(attachment);

        // Assert
        json.Should().Contain("\"type\":\"image\"");
        json.Should().Contain("\"photo\"");
        json.Should().Contain("\"file_id\":\"file123\"");
    }

    [Fact]
    public void VideoAttachment_ShouldSerialize_ToJson()
    {
        // Arrange
        var attachment = new VideoAttachment
        {
            Video = new Video
            {
                Id = 123,
                FileId = "video123",
                Width = 1280,
                Height = 720
            }
        };

        // Act
        var json = MaxJsonSerializer.Serialize<Attachment>(attachment);

        // Assert
        json.Should().Contain("\"type\":\"file\"");
        json.Should().Contain("\"video\"");
        json.Should().Contain("\"file_id\":\"video123\"");
    }
}


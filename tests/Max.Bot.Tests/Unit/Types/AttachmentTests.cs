using System.Text.Json;
using FluentAssertions;
using Max.Bot.Networking;
using Max.Bot.Types;
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
        attachment.Type.Should().Be("image");
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
        attachment.Type.Should().Be("file");
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
        attachment.Type.Should().Be("file");
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
        attachment.Type.Should().Be("file");
        var documentAttachment = (DocumentAttachment)attachment;
        documentAttachment.Document.Should().NotBeNull();
        documentAttachment.Document.Id.Should().Be(123);
        documentAttachment.Document.FileId.Should().Be("doc123");
        documentAttachment.Document.FileName.Should().Be("document.pdf");
    }

    [Fact]
    public void LocationAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"type":"location","latitude":55.753460,"longitude":37.621602}""";

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<LocationAttachment>();
        attachment.Type.Should().Be("location");
        var locationAttachment = (LocationAttachment)attachment;
        locationAttachment.Latitude.Should().Be(55.75346);
        locationAttachment.Longitude.Should().Be(37.621602);
    }

    [Fact]
    public void ContactAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """{"type":"contact","payload":{"vcf_info":"vcf contact here","max_info":{"user_id":123,"username":"username here","first_name":"first name here","last_name":"last name here","is_bot":false,"last_activity_time":1769505660}}}""";

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<ContactAttachment>();
        attachment.Type.Should().Be("contact");
        var contactAttachment = (ContactAttachment)attachment;
        contactAttachment.Payload.Should().NotBeNull();
        contactAttachment.Payload.VcfInfo.Should().Be("vcf contact here");
        contactAttachment.Payload.MaxInfo.Should().NotBeNull();
        contactAttachment.Payload.MaxInfo!.Id.Should().Be(123);
        contactAttachment.Payload.MaxInfo.Username.Should().Be("username here");
        contactAttachment.Payload.MaxInfo.FirstName.Should().Be("first name here");
        contactAttachment.Payload.MaxInfo.LastName.Should().Be("last name here");
        contactAttachment.Payload.MaxInfo.IsBot.Should().Be(false);
        contactAttachment.Payload.MaxInfo.LastActivityTime.Should().Be(1769505660);
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

    [Fact]
    public void ContactAttachment_ShouldSerialize_ToJson()
    {
        // Arrange
        var attachment = new ContactAttachment
        {
            Payload = new Contact
            {
                VcfInfo = "vcf contact here",
                MaxInfo = new ContactInfo
                {
                    Id = 123,
                    Username = "username here",
                    FirstName = "first name here",
                    LastName = "last name here",
                    IsBot = false,
                    LastActivityTime = 1769505660,
                },
            },
        };

        // Act
        var json = MaxJsonSerializer.Serialize<Attachment>(attachment);

        // Assert
        json.Should().Contain("\"user_id\":123");
        json.Should().Contain("\"username\":\"username here\"");
        json.Should().Contain("\"first_name\":\"first name here\"");
        json.Should().Contain("\"last_name\":\"last name here\"");
        json.Should().Contain("\"is_bot\":false");
        json.Should().Contain("\"last_activity_time\":1769505660");
    }

    [Fact]
    public void InlineKeyboardAttachment_ShouldDeserialize_FromJson()
    {
        // Arrange
        var json = """
            {
                "type":"inline_keyboard",
                "callback_id":"cb123",
                "payload":{
                    "buttons":[
                        [
                            {"text":"❤️ Меры поддержки","type":"message"}
                        ]
                    ]
                }
            }
            """;

        // Act
        var attachment = MaxJsonSerializer.Deserialize<Attachment>(json);

        // Assert
        attachment.Should().NotBeNull();
        attachment.Should().BeOfType<InlineKeyboardAttachment>();
        attachment.Type.Should().Be("inline_keyboard");
        var keyboardAttachment = (InlineKeyboardAttachment)attachment;
        keyboardAttachment.CallbackId.Should().Be("cb123");
        keyboardAttachment.Payload.Should().NotBeNull();
        keyboardAttachment.Payload!.Should().ContainKey("buttons");
        var buttonsElement = (JsonElement)keyboardAttachment.Payload!["buttons"];
        buttonsElement.ValueKind.Should().Be(JsonValueKind.Array);
        buttonsElement.GetArrayLength().Should().Be(1);
    }
}


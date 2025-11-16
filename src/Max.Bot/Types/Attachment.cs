// 📁 [Attachment] - Модель вложения в Max Messenger
// 🎯 Core function: Представляет вложение сообщения (полиморфная модель)
// 🔗 Key dependencies: System.Text.Json.Serialization, System.ComponentModel.DataAnnotations, Max.Bot.Types.Enums
// 💡 Usage: Используется в Message для представления вложений сообщения

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types;

/// <summary>
/// Represents an attachment in a Max Messenger message.
/// This is a polymorphic type that can contain different media types (Photo, Video, Audio, Document).
/// </summary>
[JsonConverter(typeof(Converters.AttachmentJsonConverter))]
public abstract class Attachment
{
    /// <summary>
    /// Gets or sets the type of the attachment.
    /// </summary>
    /// <value>The type of the attachment (text, image, or file).</value>
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }
}

/// <summary>
/// Represents a photo attachment.
/// </summary>
public class PhotoAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the photo in this attachment.
    /// </summary>
    /// <value>The photo object.</value>
    [JsonPropertyName("photo")]
    public Photo Photo { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotoAttachment"/> class.
    /// </summary>
    public PhotoAttachment()
    {
        Type = MessageType.Image;
    }
}

/// <summary>
/// Represents a video attachment.
/// </summary>
public class VideoAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the video in this attachment.
    /// </summary>
    /// <value>The video object.</value>
    [JsonPropertyName("video")]
    public Video Video { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoAttachment"/> class.
    /// </summary>
    public VideoAttachment()
    {
        Type = MessageType.File;
    }
}

/// <summary>
/// Represents an audio attachment.
/// </summary>
public class AudioAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the audio in this attachment.
    /// </summary>
    /// <value>The audio object.</value>
    [JsonPropertyName("audio")]
    public Audio Audio { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioAttachment"/> class.
    /// </summary>
    public AudioAttachment()
    {
        Type = MessageType.File;
    }
}

/// <summary>
/// Represents a document attachment.
/// </summary>
public class DocumentAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the document in this attachment.
    /// </summary>
    /// <value>The document object.</value>
    [JsonPropertyName("document")]
    public Document Document { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentAttachment"/> class.
    /// </summary>
    public DocumentAttachment()
    {
        Type = MessageType.File;
    }
}


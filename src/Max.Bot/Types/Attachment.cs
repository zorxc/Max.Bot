using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    /// <value>The type of the attachment (text, image, file, inline_keyboard, etc.).</value>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
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
    public PhotoAttachment() => Type = AttachmentTypeNames.Image;
}

/// <summary>
/// Represents an image attachment.
/// </summary>
public class ImageAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the image in this attachment.
    /// </summary>
    /// <value>The photo object.</value>
    [JsonPropertyName("payload")]
    public Image Payload { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageAttachment"/> class.
    /// </summary>
    public ImageAttachment() => Type = AttachmentTypeNames.Image;
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
    public VideoAttachment() => Type = AttachmentTypeNames.File;
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
    public AudioAttachment() => Type = AttachmentTypeNames.File;
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
    public DocumentAttachment() => Type = AttachmentTypeNames.File;
}

/// <summary>
/// Represents a location attachment.
/// </summary>
public class LocationAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the location's latitude.
    /// </summary>
    /// <value>The latitude value.</value>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the location's longitude.
    /// </summary>
    /// <value>The longitude value.</value>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationAttachment"/> class.
    /// </summary>
    public LocationAttachment() => Type = AttachmentTypeNames.Location;
}

/// <summary>
/// Represents a contact attachment.
/// </summary>
public class ContactAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the contact in this attachment.
    /// </summary>
    [JsonPropertyName("payload")]
    public Contact Payload { get; set; } = null!;
}

/// <summary>
/// Represents an inline keyboard attachment.
/// </summary>
public class InlineKeyboardAttachment : Attachment
{
    /// <summary>
    /// Gets or sets the callback ID for this keyboard attachment.
    /// </summary>
    /// <value>The callback ID, or null if not available.</value>
    [JsonPropertyName("callback_id")]
    public string? CallbackId { get; set; }

    /// <summary>
    /// Gets or sets the payload containing the keyboard buttons.
    /// </summary>
    /// <value>The payload containing buttons, or null if not available.</value>
    [JsonPropertyName("payload")]
    public Dictionary<string, object>? Payload { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboardAttachment"/> class.
    /// </summary>
    public InlineKeyboardAttachment() => Type = AttachmentTypeNames.InlineKeyboard;
}

internal static class AttachmentTypeNames
{
    public const string Image = "image";
    public const string File = "file";
    public const string InlineKeyboard = "inline_keyboard";
    public const string Location = "location";
    public const string Contact = "contact";
}


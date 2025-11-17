using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types.Requests;

/// <summary>
/// Represents a request to create an attachment for a message.
/// </summary>
public class AttachmentRequest
{
    /// <summary>
    /// Gets or sets the type of the attachment.
    /// </summary>
    /// <value>The type of the attachment (image, video, audio, or file).</value>
    [Required(ErrorMessage = "Attachment type is required.")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the payload of the attachment.
    /// </summary>
    /// <value>
    /// The payload object containing attachment data (e.g., token for video/audio,
    /// or JSON object returned after file upload for image/file).
    /// </value>
    [Required(ErrorMessage = "Attachment payload is required.")]
    [JsonPropertyName("payload")]
    public object Payload { get; set; } = null!;
}


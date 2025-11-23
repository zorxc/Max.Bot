using System.Text.Json.Serialization;

namespace Max.Bot.Types.Enums;

/// <summary>
/// Represents the type of file to upload.
/// </summary>
public enum UploadType
{
    /// <summary>
    /// Image file.
    /// </summary>
    [JsonPropertyName("image")]
    Image,

    /// <summary>
    /// Video file.
    /// </summary>
    [JsonPropertyName("video")]
    Video,

    /// <summary>
    /// Audio file.
    /// </summary>
    [JsonPropertyName("audio")]
    Audio,

    /// <summary>
    /// Generic file.
    /// </summary>
    [JsonPropertyName("file")]
    File
}







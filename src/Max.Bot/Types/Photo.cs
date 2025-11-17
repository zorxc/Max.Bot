using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents a photo in Max Messenger.
/// </summary>
public class Photo
{
    /// <summary>
    /// Gets or sets the unique identifier of the photo.
    /// </summary>
    /// <value>The unique identifier of the photo.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Photo ID must be greater than zero.")]
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the file identifier of the photo.
    /// </summary>
    /// <value>The file identifier of the photo.</value>
    [Required(ErrorMessage = "File ID is required.")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "File ID must be between 1 and 256 characters.")]
    [JsonPropertyName("file_id")]
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width of the photo in pixels.
    /// </summary>
    /// <value>The width of the photo in pixels.</value>
    [Range(1, int.MaxValue, ErrorMessage = "Width must be greater than zero.")]
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the photo in pixels.
    /// </summary>
    /// <value>The height of the photo in pixels.</value>
    [Range(1, int.MaxValue, ErrorMessage = "Height must be greater than zero.")]
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the size of the photo file in bytes.
    /// </summary>
    /// <value>The size of the photo file in bytes, or null if not available.</value>
    [Range(1, long.MaxValue, ErrorMessage = "File size must be greater than zero if provided.")]
    [JsonPropertyName("file_size")]
    public long? FileSize { get; set; }

    /// <summary>
    /// Gets or sets the URL of the photo.
    /// </summary>
    /// <value>The URL of the photo, or null if not available.</value>
    [Url(ErrorMessage = "URL must be a valid URL if provided.")]
    [StringLength(2048, ErrorMessage = "URL must not exceed 2048 characters.")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}


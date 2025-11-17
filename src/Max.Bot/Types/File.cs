using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents a file in Max Messenger.
/// </summary>
public class File
{
    /// <summary>
    /// Gets or sets the unique identifier of the file.
    /// </summary>
    /// <value>The unique identifier of the file.</value>
    [Required(ErrorMessage = "File ID is required.")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "File ID must be between 1 and 256 characters.")]
    [JsonPropertyName("file_id")]
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    /// <value>The size of the file in bytes, or null if not available.</value>
    [Range(1, long.MaxValue, ErrorMessage = "File size must be greater than zero if provided.")]
    [JsonPropertyName("file_size")]
    public long? FileSize { get; set; }

    /// <summary>
    /// Gets or sets the file path or URL.
    /// </summary>
    /// <value>The file path or URL, or null if not available.</value>
    [StringLength(2048, ErrorMessage = "File path must not exceed 2048 characters.")]
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }
}


// 📁 [CallbackQuery] - Модель callback query в Max Messenger
// 🎯 Core function: Представляет callback query от inline кнопки
// 🔗 Key dependencies: System.Text.Json.Serialization, System.ComponentModel.DataAnnotations
// 💡 Usage: Используется в Update для представления callback query

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents a callback query from an inline button press.
/// </summary>
public class CallbackQuery
{
    /// <summary>
    /// Gets or sets the unique identifier of the callback query.
    /// </summary>
    /// <value>The unique identifier of the callback query.</value>
    [Required(ErrorMessage = "Callback query ID is required.")]
    [StringLength(64, MinimumLength = 1, ErrorMessage = "Callback query ID must be between 1 and 64 characters.")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who pressed the button.
    /// </summary>
    /// <value>The user who pressed the button.</value>
    [Required(ErrorMessage = "From user is required.")]
    [JsonPropertyName("from")]
    public User From { get; set; } = null!;

    /// <summary>
    /// Gets or sets the message with the inline button that was pressed.
    /// </summary>
    /// <value>The message with the inline button, or null if not available.</value>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    /// <summary>
    /// Gets or sets the data associated with the callback button.
    /// </summary>
    /// <value>The callback data, or null if not available.</value>
    [StringLength(64, ErrorMessage = "Callback data must not exceed 64 characters.")]
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}


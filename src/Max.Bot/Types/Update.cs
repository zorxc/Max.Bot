// 📁 [Update] - Модель обновления Max Messenger
// 🎯 Core function: Представляет обновление от Max Messenger
// 🔗 Key dependencies: System.Text.Json.Serialization, System.ComponentModel.DataAnnotations, Max.Bot.Types, Max.Bot.Types.Enums
// 💡 Usage: Используется для получения обновлений от Max Messenger API

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Max.Bot.Types.Enums;

namespace Max.Bot.Types;

/// <summary>
/// Represents an update from Max Messenger.
/// </summary>
public class Update
{
    /// <summary>
    /// Gets or sets the unique identifier of the update.
    /// </summary>
    /// <value>The unique identifier of the update.</value>
    [Range(1, long.MaxValue, ErrorMessage = "Update ID must be greater than zero.")]
    [JsonPropertyName("updateId")]
    public long UpdateId { get; set; }

    /// <summary>
    /// Gets or sets the type of the update.
    /// </summary>
    /// <value>The type of the update (message or callback_query).</value>
    [JsonPropertyName("type")]
    public UpdateType Type { get; set; }

    /// <summary>
    /// Gets or sets the message in this update (if type is Message).
    /// </summary>
    /// <value>The message in this update, or null if not available.</value>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }

    /// <summary>
    /// Gets or sets the callback query in this update (if type is CallbackQuery).
    /// </summary>
    /// <value>The callback query in this update, or null if not available.</value>
    [JsonPropertyName("callbackQuery")]
    public CallbackQuery? CallbackQuery { get; set; }
}


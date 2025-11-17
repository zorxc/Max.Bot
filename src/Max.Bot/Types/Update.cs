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
    /// <remarks>
    /// API returns "update_type" field (e.g., "message_created"), but we map it to UpdateType enum.
    /// If update_type is not present, we infer type from presence of "message" or "callbackQuery" fields.
    /// </remarks>
    [JsonPropertyName("update_type")]
    public string? UpdateTypeRaw { get; set; }

    /// <summary>
    /// Gets or sets the type of the update as enum.
    /// </summary>
    /// <value>The type of the update (message or callback_query).</value>
    [JsonIgnore]
    public UpdateType Type
    {
        get
        {
            // * Infer type from update_type field or from presence of message/callbackQuery
            if (!string.IsNullOrEmpty(UpdateTypeRaw))
            {
                if (UpdateTypeRaw.Contains("message", StringComparison.OrdinalIgnoreCase))
                {
                    return UpdateType.Message;
                }
                if (UpdateTypeRaw.Contains("callback", StringComparison.OrdinalIgnoreCase))
                {
                    return UpdateType.CallbackQuery;
                }
            }
            
            // * Fallback: infer from presence of message or callbackQuery fields
            if (Message != null)
            {
                return UpdateType.Message;
            }
            if (CallbackQuery != null)
            {
                return UpdateType.CallbackQuery;
            }
            
            return UpdateType.Message; // Default to Message
        }
    }

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


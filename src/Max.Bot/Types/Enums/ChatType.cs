using System.Text.Json.Serialization;

namespace Max.Bot.Types.Enums;

/// <summary>
/// Represents the type of a chat.
/// Maps to the official Max API chat type values.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatType
{
    /// <summary>
    /// Group chat (type: "chat").
    /// </summary>
    [JsonPropertyName("chat")]
    Chat,

    /// <summary>
    /// Private dialog with a user (type: "dialog").
    /// </summary>
    [JsonPropertyName("dialog")]
    Dialog,

    /// <summary>
    /// Channel (type: "channel").
    /// </summary>
    [JsonPropertyName("channel")]
    Channel,

    #region Backward Compatibility Aliases

    /// <summary>
    /// Alias for Dialog. Use Dialog instead.
    /// </summary>
    [Obsolete("Use Dialog instead. This alias will be removed in future versions.")]
    Private = Dialog,

    /// <summary>
    /// Alias for Chat. Use Chat instead.
    /// </summary>
    [Obsolete("Use Chat instead. This alias will be removed in future versions.")]
    Group = Chat

    #endregion
}

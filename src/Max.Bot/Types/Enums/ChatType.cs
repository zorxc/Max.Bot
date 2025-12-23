// 📁 ChatType.cs - Chat type enum for API JSON
// 🎯 Core function: Maps Max API chat type strings to enum values.
// 🔗 Key dependencies: System.Text.Json enum converter attributes.
// 💡 Usage: Used by Chat and other chat-related payloads.

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
}

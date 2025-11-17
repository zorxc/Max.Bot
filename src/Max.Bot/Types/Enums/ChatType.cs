using System.Text.Json.Serialization;

namespace Max.Bot.Types.Enums;

/// <summary>
/// Represents the type of a chat.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatType
{
    /// <summary>
    /// Private chat with a user.
    /// </summary>
    Private,

    /// <summary>
    /// Group chat.
    /// </summary>
    Group,

    /// <summary>
    /// Channel.
    /// </summary>
    Channel
}


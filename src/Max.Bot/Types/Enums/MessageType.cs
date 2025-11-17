using System.Text.Json.Serialization;

namespace Max.Bot.Types.Enums;

/// <summary>
/// Represents the type of a message.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    /// <summary>
    /// Text message.
    /// </summary>
    Text,

    /// <summary>
    /// Image message.
    /// </summary>
    Image,

    /// <summary>
    /// File message.
    /// </summary>
    File
}


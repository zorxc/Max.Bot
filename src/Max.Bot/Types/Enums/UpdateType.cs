using System.Text.Json.Serialization;

namespace Max.Bot.Types.Enums;

/// <summary>
/// Represents the type of an update.
/// </summary>
public enum UpdateType
{
    /// <summary>
    /// New message update.
    /// </summary>
    Message,

    /// <summary>
    /// Callback query update.
    /// </summary>
    CallbackQuery
}


using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Max.Bot.Types;

/// <summary>
/// Represents an inline keyboard with buttons arranged in rows.
/// </summary>
public class InlineKeyboard
{
    /// <summary>
    /// Gets or sets the buttons arranged in rows.
    /// Each inner array represents a row of buttons.
    /// </summary>
    /// <value>An array of button rows, where each row is an array of buttons.</value>
    [JsonPropertyName("inline_keyboard")]
    public InlineKeyboardButton[][] Buttons { get; set; } = Array.Empty<InlineKeyboardButton[]>();

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboard"/> class.
    /// </summary>
    public InlineKeyboard()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboard"/> class with the specified buttons.
    /// </summary>
    /// <param name="buttons">The buttons arranged in rows.</param>
    public InlineKeyboard(InlineKeyboardButton[][] buttons)
    {
        Buttons = buttons ?? throw new ArgumentNullException(nameof(buttons));
    }
}


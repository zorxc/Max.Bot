using System;
using System.IO;

namespace Max.Bot.Examples;

/// <summary>
/// Represents runtime settings required to execute the sample bots.
/// </summary>
public sealed class SampleSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SampleSettings"/> class.
    /// </summary>
    /// <param name="token">The bot token used to authenticate API calls.</param>
    /// <param name="defaultChatId">Optional chat identifier used by samples that need a target chat.</param>
    /// <param name="uploadFilePath">Optional path to a local file for upload scenarios.</param>
    public SampleSettings(string token, long? defaultChatId, string? uploadFilePath)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        DefaultChatId = defaultChatId;
        UploadFilePath = uploadFilePath;
    }

    /// <summary>
    /// Gets the bot token.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Gets the optional default chat identifier used for proactive messages.
    /// </summary>
    public long? DefaultChatId { get; }

    /// <summary>
    /// Gets the optional upload file path used by file samples.
    /// </summary>
    public string? UploadFilePath { get; }

    /// <summary>
    /// Loads settings from environment variables.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when mandatory values are missing or invalid.</exception>
    public static SampleSettings LoadFromEnvironment()
    {
        var token = Environment.GetEnvironmentVariable("MAX_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Environment variable MAX_BOT_TOKEN is required. Store secrets in Secret Manager or CI secrets.");
        }

        long? chatId = null;
        var chatIdValue = Environment.GetEnvironmentVariable("MAX_BOT_CHAT_ID");
        if (!string.IsNullOrWhiteSpace(chatIdValue))
        {
            if (!long.TryParse(chatIdValue, out var parsedChatId) || parsedChatId <= 0)
            {
                throw new InvalidOperationException("MAX_BOT_CHAT_ID must be a positive integer.");
            }

            chatId = parsedChatId;
        }

        var uploadFilePath = Environment.GetEnvironmentVariable("MAX_BOT_FILE");
        if (!string.IsNullOrWhiteSpace(uploadFilePath) && !File.Exists(uploadFilePath))
        {
            throw new InvalidOperationException($"MAX_BOT_FILE points to '{uploadFilePath}', but the file does not exist.");
        }

        return new SampleSettings(token.Trim(), chatId, string.IsNullOrWhiteSpace(uploadFilePath) ? null : uploadFilePath);
    }
}



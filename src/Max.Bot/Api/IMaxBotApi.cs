namespace Max.Bot.Api;

/// <summary>
/// Main interface for the Max Messenger Bot API.
/// </summary>
public interface IMaxBotApi
{
    /// <summary>
    /// Gets the bot-related API methods.
    /// </summary>
    /// <value>The bot API interface.</value>
    IBotApi Bot { get; }

    /// <summary>
    /// Gets the message-related API methods.
    /// </summary>
    /// <value>The messages API interface.</value>
    IMessagesApi Messages { get; }

    /// <summary>
    /// Gets the chat-related API methods.
    /// </summary>
    /// <value>The chats API interface.</value>
    IChatsApi Chats { get; }

    /// <summary>
    /// Gets the user-related API methods.
    /// </summary>
    /// <value>The users API interface.</value>
    IUsersApi Users { get; }

    /// <summary>
    /// Gets the file-related API methods.
    /// </summary>
    /// <value>The files API interface.</value>
    IFilesApi Files { get; }

    /// <summary>
    /// Gets the subscriptions/updates-related API methods.
    /// </summary>
    /// <value>The subscriptions API interface.</value>
    ISubscriptionsApi Subscriptions { get; }
}


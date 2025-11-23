using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;

namespace Max.Bot.Api;

/// <summary>
/// Interface for message-related API methods.
/// </summary>
public interface IMessagesApi
{
    /// <summary>
    /// Sends a text message to the specified chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="text">The text of the message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero, or text is null or empty.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a text message with an optional inline keyboard to the specified chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="text">The text of the message to send.</param>
    /// <param name="keyboard">The inline keyboard to attach to the message. Optional.</param>
    /// <param name="disableLinkPreview">If true, link previews will be disabled for links in the message text.</param>
    /// <param name="notify">If false, participants will not be notified. Default is true.</param>
    /// <param name="format">The text format (markdown or html).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero, or text is null or empty.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> SendMessageAsync(long chatId, string text, InlineKeyboard? keyboard, bool? disableLinkPreview = null, bool? notify = null, TextFormat? format = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message with full support for all parameters (attachments, link, format, etc.).
    /// </summary>
    /// <param name="request">The send message request containing message content and parameters.</param>
    /// <param name="chatId">The unique identifier of the chat. Optional if userId is provided.</param>
    /// <param name="userId">The unique identifier of the user. Optional if chatId is provided.</param>
    /// <param name="disableLinkPreview">If true, link previews will be disabled for links in the message text.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when both chatId and userId are provided, or neither is provided, or when chatId/userId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> SendMessageAsync(SendMessageRequest request, long? chatId = null, long? userId = null, bool? disableLinkPreview = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of messages from the specified chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of messages.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message[]> GetMessagesAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits a message in the specified chat.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to edit.</param>
    /// <param name="request">The edit request containing new message content.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> EditMessageAsync(string messageId, EditMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message from the specified chat.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is null or empty.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> DeleteMessageAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a message by its unique identifier.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is null or empty.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> GetMessageAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets video information by video token.
    /// </summary>
    /// <param name="videoToken">The video token identifier.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the video information.</returns>
    /// <exception cref="ArgumentException">Thrown when videoToken is null or empty.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Video> GetVideoAsync(string videoToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Answers a callback query from an inline keyboard button press.
    /// </summary>
    /// <param name="callbackQueryId">The unique identifier of the callback query.</param>
    /// <param name="request">The answer request containing message or notification.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when callbackQueryId is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> AnswerCallbackQueryAsync(string callbackQueryId, AnswerCallbackQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message with an attachment to the specified chat or user.
    /// </summary>
    /// <param name="attachment">The attachment to include in the message.</param>
    /// <param name="chatId">The unique identifier of the chat. Optional if userId is provided.</param>
    /// <param name="userId">The unique identifier of the user. Optional if chatId is provided.</param>
    /// <param name="text">The text of the message. Can be null if only attachment is sent.</param>
    /// <param name="disableLinkPreview">If true, link previews will be disabled for links in the message text.</param>
    /// <param name="notify">If false, participants will not be notified. Default is true.</param>
    /// <param name="format">The text format (markdown or html).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when both chatId and userId are provided, or neither is provided, or when chatId/userId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when attachment is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> SendMessageWithAttachmentAsync(AttachmentRequest attachment, long? chatId = null, long? userId = null, string? text = null, bool? disableLinkPreview = null, bool? notify = null, TextFormat? format = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forwards a message to the specified chat or user.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to forward.</param>
    /// <param name="messageChatId">The unique identifier of the chat containing the message to forward. Optional.</param>
    /// <param name="chatId">The unique identifier of the destination chat. Optional if userId is provided.</param>
    /// <param name="userId">The unique identifier of the destination user. Optional if chatId is provided.</param>
    /// <param name="text">Additional text to include with the forwarded message. Optional.</param>
    /// <param name="disableLinkPreview">If true, link previews will be disabled for links in the message text.</param>
    /// <param name="notify">If false, participants will not be notified. Default is true.</param>
    /// <param name="format">The text format (markdown or html).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is less than or equal to zero, both chatId and userId are provided, or neither is provided, or when chatId/userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> ForwardMessageAsync(long messageId, long? messageChatId = null, long? chatId = null, long? userId = null, string? text = null, bool? disableLinkPreview = null, bool? notify = null, TextFormat? format = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replies to a message in the specified chat.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to reply to.</param>
    /// <param name="text">The text of the reply message.</param>
    /// <param name="messageChatId">The unique identifier of the chat containing the message to reply to. Optional.</param>
    /// <param name="chatId">The unique identifier of the chat. Optional if userId is provided.</param>
    /// <param name="userId">The unique identifier of the user. Optional if chatId is provided.</param>
    /// <param name="disableLinkPreview">If true, link previews will be disabled for links in the message text.</param>
    /// <param name="notify">If false, participants will not be notified. Default is true.</param>
    /// <param name="format">The text format (markdown or html).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is less than or equal to zero, text is null or empty, both chatId and userId are provided, or neither is provided, or when chatId/userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message> ReplyToMessageAsync(long messageId, string text, long? messageChatId = null, long? chatId = null, long? userId = null, bool? disableLinkPreview = null, bool? notify = null, TextFormat? format = null, CancellationToken cancellationToken = default);
}


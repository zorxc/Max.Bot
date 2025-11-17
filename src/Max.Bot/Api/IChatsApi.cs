using Max.Bot.Types;
using Max.Bot.Types.Requests;

namespace Max.Bot.Api;

/// <summary>
/// Interface for chat-related API methods.
/// </summary>
public interface IChatsApi
{
    /// <summary>
    /// Gets information about a chat by its identifier.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the chat information.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Chat> GetChatAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all chats for the bot.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of chats.</returns>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Chat[]> GetChatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about a chat by its public link or username.
    /// </summary>
    /// <param name="chatLink">The public link or username of the chat (pattern: @?[a-zA-Z]+[a-zA-Z0-9-_]*).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the chat information.</returns>
    /// <exception cref="ArgumentException">Thrown when chatLink is null or empty.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Chat> GetChatByLinkAsync(string chatLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates chat information.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="request">The update request containing new chat information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated chat information.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Chat> UpdateChatAsync(long chatId, UpdateChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> DeleteChatAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a chat action (typing, sending photo, etc.).
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="request">The action request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> SendChatActionAsync(long chatId, SendChatActionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the pinned message in a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the pinned message.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Message?> GetPinnedMessageAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pins a message in a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="request">The pin request containing the message ID.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> PinMessageAsync(long chatId, PinMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpins the pinned message in a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> UnpinMessageAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about the bot's membership in a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the membership information.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Chat> GetChatMembershipAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Leaves a chat (removes the bot from the chat).
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> LeaveChatAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of chat administrators.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of administrators.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<User[]> GetChatAdminsAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an admin to a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="request">The request containing the user ID to make an admin.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> AddChatAdminAsync(long chatId, AddChatAdminRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes admin rights from a user in a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="userId">The unique identifier of the user to remove admin rights from.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId or userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> RemoveChatAdminAsync(long chatId, long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of chat members.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="offset">The offset for pagination (optional).</param>
    /// <param name="limit">The limit for pagination (optional).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of members.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<User[]> GetChatMembersAsync(long chatId, int? offset = null, int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds members to a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="request">The request containing the user IDs to add.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId is less than or equal to zero.</exception>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> AddChatMembersAsync(long chatId, AddChatMembersRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a member from a chat.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat.</param>
    /// <param name="userId">The unique identifier of the user to remove.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when chatId or userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    Task<Response> RemoveChatMemberAsync(long chatId, long userId, CancellationToken cancellationToken = default);
}


using System.Collections.Generic;
using System.Net.Http;
using Max.Bot.Configuration;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Requests;

namespace Max.Bot.Api;

/// <summary>
/// Implementation of chat-related API methods.
/// </summary>
internal class ChatsApi : BaseApi, IChatsApi
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatsApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">The bot options containing token and base URL.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    public ChatsApi(IMaxHttpClient httpClient, MaxBotOptions options)
        : base(httpClient, options)
    {
    }

    /// <inheritdoc />
    public async Task<Chat> GetChatAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Get, $"/chats/{chatId}");
        return await ExecuteRequestAsync<Chat>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Chat[]> GetChatsAsync(CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, "/chats");
        return await ExecuteRequestAsync<Chat[]>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Chat> GetChatByLinkAsync(string chatLink, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(chatLink))
        {
            throw new ArgumentException("Chat link cannot be null or empty.", nameof(chatLink));
        }

        var request = CreateRequest(HttpMethod.Get, $"/chats/{Uri.EscapeDataString(chatLink)}");
        return await ExecuteRequestAsync<Chat>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Chat> UpdateChatAsync(long chatId, UpdateChatRequest request, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = CreateRequest(HttpMethod.Patch, $"/chats/{chatId}", request);
        return await ExecuteRequestAsync<Chat>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> DeleteChatAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Delete, $"/chats/{chatId}");
        return await ExecuteRequestAsync<Response>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> SendChatActionAsync(long chatId, SendChatActionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = CreateRequest(HttpMethod.Post, $"/chats/{chatId}/actions", request);
        return await ExecuteRequestAsync<Response>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Message?> GetPinnedMessageAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Get, $"/chats/{chatId}/pin");
        var response = await HttpClient.SendAsync<Response<Message>>(request, cancellationToken).ConfigureAwait(false);

        if (response == null || !response.Ok)
        {
            if (response != null && !response.Ok)
            {
                throw new Exceptions.MaxApiException(
                    "API request failed. The response indicates an error.",
                    null,
                    System.Net.HttpStatusCode.BadRequest);
            }
            return null;
        }

        return response.Result;
    }

    /// <inheritdoc />
    public async Task<Response> PinMessageAsync(long chatId, PinMessageRequest request, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = CreateRequest(HttpMethod.Put, $"/chats/{chatId}/pin", request);
        return await ExecuteRequestAsync<Response>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> UnpinMessageAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Delete, $"/chats/{chatId}/pin");
        return await ExecuteRequestAsync<Response>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Chat> GetChatMembershipAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Get, $"/chats/{chatId}/members/me");
        return await ExecuteRequestAsync<Chat>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> LeaveChatAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Delete, $"/chats/{chatId}/members/me");
        return await ExecuteRequestAsync<Response>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<User[]> GetChatAdminsAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var request = CreateRequest(HttpMethod.Get, $"/chats/{chatId}/members/admins");
        return await ExecuteRequestAsync<User[]>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> AddChatAdminAsync(long chatId, AddChatAdminRequest request, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = CreateRequest(HttpMethod.Post, $"/chats/{chatId}/members/admins", request);
        return await ExecuteRequestAsync<Response>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> RemoveChatAdminAsync(long chatId, long userId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
        }

        var request = CreateRequest(HttpMethod.Delete, $"/chats/{chatId}/members/admins/{userId}");
        return await ExecuteRequestAsync<Response>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<User[]> GetChatMembersAsync(long chatId, int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);

        var queryParams = new Dictionary<string, string?>();
        if (offset.HasValue)
        {
            queryParams["offset"] = offset.Value.ToString();
        }
        if (limit.HasValue)
        {
            queryParams["limit"] = limit.Value.ToString();
        }

        var request = CreateRequest(HttpMethod.Get, $"/chats/{chatId}/members", null, queryParams.Count > 0 ? queryParams : null);
        return await ExecuteRequestAsync<User[]>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> AddChatMembersAsync(long chatId, AddChatMembersRequest request, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        ArgumentNullException.ThrowIfNull(request);

        var apiRequest = CreateRequest(HttpMethod.Post, $"/chats/{chatId}/members", request);
        return await ExecuteRequestAsync<Response>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> RemoveChatMemberAsync(long chatId, long userId, CancellationToken cancellationToken = default)
    {
        ValidateChatId(chatId);
        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
        }

        var queryParams = new Dictionary<string, string?>
        {
            { "user_id", userId.ToString() }
        };

        var request = CreateRequest(HttpMethod.Delete, $"/chats/{chatId}/members", null, queryParams);
        return await ExecuteRequestAsync<Response>(request, cancellationToken).ConfigureAwait(false);
    }
}


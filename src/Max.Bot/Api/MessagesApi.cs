using System.Linq;
using System.Net.Http;
using Max.Bot.Configuration;
using Max.Bot.Networking;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;

namespace Max.Bot.Api;

/// <summary>
/// Implementation of message-related API methods.
/// </summary>
internal class MessagesApi : BaseApi, IMessagesApi
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagesApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">The bot options containing token and base URL.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    public MessagesApi(IMaxHttpClient httpClient, MaxBotOptions options)
        : base(httpClient, options)
    {
    }

    /// <inheritdoc />
    public async Task<Message> SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        ValidateNotEmpty(text, nameof(text));

        // * Use SendMessageRequest format with chat_id in query parameters (same as second method)
        var sendRequest = new SendMessageRequest
        {
            Text = text
        };

        var queryParams = new Dictionary<string, string?>
        {
            { "chat_id", chatId.ToString() }
        };

        var request = CreateRequest(HttpMethod.Post, "/messages", sendRequest, queryParams);

        // * POST /messages returns {"message":{...}}, not {"ok":true,"result":{...}}
        // We need to handle this special format
        var messageResponse = await HttpClient.SendAsync<MessageResponse>(request, cancellationToken).ConfigureAwait(false);
        if (messageResponse?.Message == null)
        {
            throw new Exceptions.MaxApiException(
                "API request returned null message in response.",
                null,
                System.Net.HttpStatusCode.BadRequest);
        }
        return messageResponse.Message;
    }

    /// <inheritdoc />
    public async Task<Message> SendMessageAsync(long chatId, string text, InlineKeyboard? keyboard, bool? disableLinkPreview = null, bool? notify = null, TextFormat? format = null, CancellationToken cancellationToken = default)
    {
        ValidateNotEmpty(text, nameof(text));

        var sendRequest = new SendMessageRequest
        {
            Text = text,
            Notify = notify,
            Format = format
        };

        if (keyboard != null)
        {
            sendRequest.Attachments = new[]
            {
                CreateInlineKeyboardAttachment(keyboard)
            };
        }

        var queryParams = new Dictionary<string, string?>
        {
            { "chat_id", chatId.ToString() }
        };

        if (disableLinkPreview.HasValue)
        {
            queryParams["disable_link_preview"] = disableLinkPreview.Value.ToString().ToLowerInvariant();
        }

        var request = CreateRequest(HttpMethod.Post, "/messages", sendRequest, queryParams);

        var messageResponse = await HttpClient.SendAsync<MessageResponse>(request, cancellationToken).ConfigureAwait(false);
        if (messageResponse?.Message == null)
        {
            throw new Exceptions.MaxApiException(
                "API request returned null message in response.",
                null,
                System.Net.HttpStatusCode.BadRequest);
        }
        return messageResponse.Message;
    }

    /// <inheritdoc />
    public async Task<Message> SendMessageToUserAsync(long userId, string text, InlineKeyboard? keyboard = null, bool? disableLinkPreview = null, bool? notify = null, TextFormat? format = null, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateNotEmpty(text, nameof(text));

        var sendRequest = new SendMessageRequest
        {
            Text = text,
            Notify = notify,
            Format = format
        };

        if (keyboard != null)
        {
            sendRequest.Attachments = new[]
            {
                CreateInlineKeyboardAttachment(keyboard)
            };
        }

        return await SendMessageAsync(sendRequest, chatId: null, userId: userId, disableLinkPreview, cancellationToken).ConfigureAwait(false);
    }


    /// <inheritdoc />
    public async Task<Message> SendMessageAsync(SendMessageRequest request, long? chatId = null, long? userId = null, bool? disableLinkPreview = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate that exactly one of chatId or userId is provided
        if (chatId.HasValue && userId.HasValue)
        {
            throw new ArgumentException("Cannot specify both chatId and userId. Provide either chatId or userId, but not both.");
        }

        if (!chatId.HasValue && !userId.HasValue)
        {
            throw new ArgumentException("Either chatId or userId must be provided.");
        }

        if (userId.HasValue)
        {
            ValidateUserId(userId.Value);
        }

        // Build query parameters
        var queryParams = new Dictionary<string, string?>();
        if (chatId.HasValue)
        {
            queryParams["chat_id"] = chatId.Value.ToString();
        }

        if (userId.HasValue)
        {
            queryParams["user_id"] = userId.Value.ToString();
        }

        if (disableLinkPreview.HasValue)
        {
            queryParams["disable_link_preview"] = disableLinkPreview.Value.ToString().ToLowerInvariant();
        }

        var apiRequest = CreateRequest(HttpMethod.Post, "/messages", request, queryParams);

        // * POST /messages returns {"message":{...}}, not {"ok":true,"result":{...}}
        // We need to handle this special format
        var messageResponse = await HttpClient.SendAsync<MessageResponse>(apiRequest, cancellationToken).ConfigureAwait(false);
        if (messageResponse?.Message == null)
        {
            throw new Exceptions.MaxApiException(
                "API request returned null message in response.",
                null,
                System.Net.HttpStatusCode.BadRequest);
        }
        return messageResponse.Message;
    }

    /// <inheritdoc />
    public async Task<Message[]> GetMessagesAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "chat_id", chatId.ToString() }
        };

        var request = CreateRequest(HttpMethod.Get, "/messages", null, queryParams);
        var response = await ExecuteRequestAsync<GetMessagesResponse>(request, cancellationToken).ConfigureAwait(false);
        return response.Messages;
    }

    /// <inheritdoc />
    public async Task<Response> EditMessageAsync(string messageId, EditMessageRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        }

        ArgumentNullException.ThrowIfNull(request);

        var queryParams = new Dictionary<string, string?>
        {
            { "message_id", messageId }
        };

        var apiRequest = CreateRequest(HttpMethod.Put, "/messages", request, queryParams);
        return await ExecuteRequestAsync<Response>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> EditMessageReplyMarkupAsync(string messageId, InlineKeyboard? keyboard = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        }

        // Получаем текущее сообщение, чтобы сохранить другие вложения (изображения, файлы и т.д.)
        var currentMessage = await GetMessageAsync(messageId, cancellationToken).ConfigureAwait(false);
        var attachments = currentMessage.Body?.Attachments ?? Array.Empty<Attachment>();
        var attachmentsWithoutKeyboard = attachments
            .Where(a => a is not InlineKeyboardAttachment)
            .ToList();

        var editRequest = new EditMessageRequest();

        if (keyboard == null)
        {
            // Удаляем только клавиатуру, сохраняя остальные вложения
            editRequest.Attachments = attachmentsWithoutKeyboard.ToArray();
        }
        else
        {
            // Заменяем клавиатуру новой, сохраняя остальные вложения
            attachmentsWithoutKeyboard.Add(CreateInlineKeyboardAttachmentForEdit(keyboard));
            editRequest.Attachments = attachmentsWithoutKeyboard.ToArray();
        }

        return await EditMessageAsync(messageId, editRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> DeleteMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        }

        var queryParams = new Dictionary<string, string?>
        {
            { "message_id", messageId }
        };

        var request = CreateRequest(HttpMethod.Delete, "/messages", null, queryParams);
        return await ExecuteRequestAsync<Response>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Message> GetMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        }

        var request = CreateRequest(HttpMethod.Get, $"/messages/{Uri.EscapeDataString(messageId)}", null);
        return await ExecuteRequestAsync<Message>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Video> GetVideoAsync(string videoToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(videoToken))
        {
            throw new ArgumentException("Video token cannot be null or empty.", nameof(videoToken));
        }

        var request = CreateRequest(HttpMethod.Get, $"/videos/{Uri.EscapeDataString(videoToken)}", null);
        return await ExecuteRequestAsync<Video>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> AnswerCallbackQueryAsync(string callbackQueryId, AnswerCallbackQueryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(callbackQueryId))
        {
            throw new ArgumentException("Callback query ID cannot be null or empty.", nameof(callbackQueryId));
        }

        ArgumentNullException.ThrowIfNull(request);

        var queryParams = new Dictionary<string, string?>
        {
            { "callback_query_id", callbackQueryId }
        };

        var apiRequest = CreateRequest(HttpMethod.Post, "/answers", request, queryParams);
        return await ExecuteRequestAsync<Response>(apiRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a message with an attachment to the specified chat or user.
    /// </summary>
    /// <param name="chatId">The unique identifier of the chat. Optional if userId is provided.</param>
    /// <param name="userId">The unique identifier of the user. Optional if chatId is provided.</param>
    /// <param name="text">The text of the message. Can be null if only attachment is sent.</param>
    /// <param name="attachment">The attachment to include in the message.</param>
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
    public async Task<Message> SendMessageWithAttachmentAsync(
        AttachmentRequest attachment,
        long? chatId = null,
        long? userId = null,
        string? text = null,
        bool? disableLinkPreview = null,
        bool? notify = null,
        TextFormat? format = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var request = new SendMessageRequest
        {
            Text = text,
            Attachments = new[] { attachment },
            Notify = notify,
            Format = format
        };

        return await SendMessageAsync(request, chatId, userId, disableLinkPreview, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Message> SendMessageWithAttachmentsAsync(
        IEnumerable<AttachmentRequest> attachments,
        long? chatId = null,
        long? userId = null,
        string? text = null,
        bool? disableLinkPreview = null,
        bool? notify = null,
        TextFormat? format = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachments);

        var request = new SendMessageRequest
        {
            Text = text,
            Attachments = [.. attachments],
            Notify = notify,
            Format = format
        };

        return await SendMessageAsync(request, chatId, userId, disableLinkPreview, cancellationToken).ConfigureAwait(false);
    }

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
    /// <exception cref="ArgumentException">Thrown when messageId is null or empty, both chatId and userId are provided, or neither is provided, or when chatId/userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    public async Task<Message> ForwardMessageAsync(
        string messageId,
        long? messageChatId = null,
        long? chatId = null,
        long? userId = null,
        string? text = null,
        bool? disableLinkPreview = null,
        bool? notify = null,
        TextFormat? format = null,
        CancellationToken cancellationToken = default)
    {
        ValidateNotEmpty(messageId, nameof(messageId));

        var link = new NewMessageLink
        {
            Type = MessageLinkType.Forward,
            Id = messageId,
            ChatId = messageChatId
        };

        var request = new SendMessageRequest
        {
            Text = text,
            Link = link,
            Notify = notify,
            Format = format
        };

        return await SendMessageAsync(request, chatId, userId, disableLinkPreview, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replies to a message in the specified chat.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to reply to.</param>
    /// <param name="messageChatId">The unique identifier of the chat containing the message to reply to. Optional.</param>
    /// <param name="chatId">The unique identifier of the chat. Optional if userId is provided.</param>
    /// <param name="userId">The unique identifier of the user. Optional if chatId is provided.</param>
    /// <param name="text">The text of the reply message.</param>
    /// <param name="disableLinkPreview">If true, link previews will be disabled for links in the message text.</param>
    /// <param name="notify">If false, participants will not be notified. Default is true.</param>
    /// <param name="format">The text format (markdown or html).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the sent message.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId or text is null or empty, both chatId and userId are provided, or neither is provided, or when chatId/userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    public async Task<Message> ReplyToMessageAsync(
        string messageId,
        string text,
        long? messageChatId = null,
        long? chatId = null,
        long? userId = null,
        bool? disableLinkPreview = null,
        bool? notify = null,
        TextFormat? format = null,
        CancellationToken cancellationToken = default)
    {
        ValidateNotEmpty(messageId, nameof(messageId));
        ValidateNotEmpty(text, nameof(text));

        var link = new NewMessageLink
        {
            Type = MessageLinkType.Reply,
            Id = messageId,
            ChatId = messageChatId
        };

        var request = new SendMessageRequest
        {
            Text = text,
            Link = link,
            Notify = notify,
            Format = format
        };

        return await SendMessageAsync(request, chatId, userId, disableLinkPreview, cancellationToken).ConfigureAwait(false);
    }

    private static AttachmentRequest CreateInlineKeyboardAttachment(InlineKeyboard keyboard)
    {
        ArgumentNullException.ThrowIfNull(keyboard);

        var sourceRows = keyboard.Buttons ?? Array.Empty<InlineKeyboardButton[]>();
        var normalizedRows = new InlineKeyboardButton[sourceRows.Length][];
        for (var i = 0; i < sourceRows.Length; i++)
        {
            normalizedRows[i] = sourceRows[i] ?? Array.Empty<InlineKeyboardButton>();
        }

        var payloadKeyboard = new InlineKeyboard(normalizedRows);

        return new AttachmentRequest
        {
            Type = AttachmentTypeNames.InlineKeyboard,
            Payload = payloadKeyboard
        };
    }

    private static InlineKeyboardAttachment CreateInlineKeyboardAttachmentForEdit(InlineKeyboard keyboard)
    {
        ArgumentNullException.ThrowIfNull(keyboard);

        var sourceRows = keyboard.Buttons ?? Array.Empty<InlineKeyboardButton[]>();
        var normalizedRows = new InlineKeyboardButton[sourceRows.Length][];
        for (var i = 0; i < sourceRows.Length; i++)
        {
            normalizedRows[i] = sourceRows[i] ?? Array.Empty<InlineKeyboardButton>();
        }

        var payloadKeyboard = new InlineKeyboard(normalizedRows);

        // Сериализуем клавиатуру в JSON, затем десериализуем в Dictionary для payload
        var keyboardJson = Networking.MaxJsonSerializer.Serialize(payloadKeyboard);
        var payloadDict = Networking.MaxJsonSerializer.Deserialize<Dictionary<string, object>>(keyboardJson);

        return new InlineKeyboardAttachment
        {
            Payload = payloadDict
        };
    }
}

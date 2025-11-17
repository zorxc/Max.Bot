using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Networking;
using Max.Bot.Polling;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;
using Microsoft.Extensions.Logging;

namespace Max.Bot;

/// <summary>
/// Main client for interacting with the Max Messenger Bot API.
/// </summary>
public class MaxClient : IMaxBotApi, IUpdatePipeline
{
    private readonly IMaxHttpClient _httpClient;
    private readonly MaxBotOptions _options;
    private readonly ILogger<MaxClient>? _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly HashSet<UpdateType>? _dispatchTypeFilter;
    private readonly HashSet<string>? _allowedUsernames;
    private readonly object _pollerLock = new();
    private UpdatePoller? _updatePoller;

    /// <inheritdoc />
    public IBotApi Bot { get; }

    /// <inheritdoc />
    public IMessagesApi Messages { get; }

    /// <inheritdoc />
    public IChatsApi Chats { get; }

    /// <inheritdoc />
    public IUsersApi Users { get; }

    /// <inheritdoc />
    public IFilesApi Files { get; }

    /// <inheritdoc />
    public ISubscriptionsApi Subscriptions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxClient"/> class with a bot token.
    /// </summary>
    /// <param name="token">The bot token for authentication.</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty, or options are invalid.</exception>
    public MaxClient(string token)
        : this(new MaxBotOptions { Token = token })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxClient"/> class with bot options.
    /// </summary>
    /// <param name="options">The bot options containing token and base URL.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public MaxClient(MaxBotOptions options)
        : this(options, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxClient"/> class with bot options and optional dependencies.
    /// </summary>
    /// <param name="options">The bot options containing token and base URL.</param>
    /// <param name="httpClient">Optional HTTP client. If null, a new HttpClient will be created.</param>
    /// <param name="logger">Optional logger for logging events.</param>
    /// <param name="loggerFactory">Optional logger factory used to create component-specific loggers.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public MaxClient(MaxBotOptions options, HttpClient? httpClient, ILogger<MaxClient>? logger = null, ILoggerFactory? loggerFactory = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
        _logger = logger;
        _loggerFactory = loggerFactory;
        _dispatchTypeFilter = UpdateFilterUtilities.BuildTypeFilter(_options);
        _allowedUsernames = UpdateFilterUtilities.BuildAllowedUsernames(_options);

        // Create or use provided HttpClient
        var client = httpClient ?? new HttpClient();

        // Configure MaxBotClientOptions - token is passed via Authorization header, not in URL
        // * HttpClient.Timeout must be greater than maximum long polling timeout (90s) + buffer for network delays
        // This prevents HttpClient from timing out before long polling requests complete
        var clientOptions = new MaxBotClientOptions
        {
            BaseUrl = _options.BaseUrl,
            Timeout = TimeSpan.FromSeconds(100), // 90s max polling + 10s buffer
            RetryCount = 3,
            EnableDetailedLogging = true // * Enable detailed logging for debugging
        };
        clientOptions.Validate();

        // Create MaxHttpClient
        // Note: ILogger<MaxHttpClient> requires ILoggerFactory to create
        // If logging is needed, pass ILoggerFactory to MaxClient constructor or use DI
        // For now, we pass null - MaxHttpClient will work without logging
        _httpClient = new MaxHttpClient(client, clientOptions, loggerFactory?.CreateLogger<MaxHttpClient>());

        // Initialize API groups
        Bot = new BotApi(_httpClient, _options);
        Messages = new MessagesApi(_httpClient, _options);
        Chats = new ChatsApi(_httpClient, _options);
        Users = new UsersApi(_httpClient, _options);
        Files = new FilesApi(_httpClient, _options);
        Subscriptions = new SubscriptionsApi(_httpClient, _options);
    }

    /// <summary>
    /// Starts the long polling loop with the provided <see cref="IUpdateHandler"/>.
    /// </summary>
    public async Task StartPollingAsync(IUpdateHandler handler, IServiceProvider? services = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_pollerLock)
        {
            if (_updatePoller != null)
            {
                throw new InvalidOperationException("Polling is already running. Call StopPollingAsync before starting again.");
            }

            _updatePoller = new UpdatePoller(
                this,
                Subscriptions,
                _options,
                _loggerFactory?.CreateLogger<UpdatePoller>(),
                services);
        }

        await _updatePoller.StartAsync(handler, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stops the long polling loop if it is running.
    /// </summary>
    public async Task StopPollingAsync(CancellationToken cancellationToken = default)
    {
        UpdatePoller? poller;
        lock (_pollerLock)
        {
            poller = _updatePoller;
            _updatePoller = null;
        }

        if (poller != null)
        {
            await poller.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Processes a webhook update payload using the provided handler.
    /// </summary>
    public async Task ProcessWebhookAsync(Update update, IUpdateHandler handler, IServiceProvider? services = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentNullException.ThrowIfNull(handler);

        if (!UpdateFilterUtilities.ShouldDispatch(update, _dispatchTypeFilter, _allowedUsernames))
        {
            _logger?.LogDebug("Webhook update {UpdateId} skipped by filters.", update.UpdateId);
            return;
        }

        await UpdateHandlerExecutor.ExecuteAsync(update, handler, _options, this, _logger, services, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Configures the webhook endpoint by calling <c>POST /subscriptions</c>.
    /// </summary>
    /// <param name="url">The webhook URL where updates will be sent.</param>
    /// <param name="updateTypes">Optional list of update types to receive. If null, all update types will be received.</param>
    /// <param name="secret">Optional secret that will be sent in the X-Max-Bot-Api-Secret header.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or invalid.</exception>
    public async Task<Response> ConfigureWebhookAsync(string url, List<string>? updateTypes = null, string? secret = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Webhook URL must be provided.", nameof(url));
        }

        if (_options.Webhook.EnforceHttps)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
            {
                throw new ArgumentException("Webhook URL must be an absolute URI.", nameof(url));
            }

            if (!Uri.UriSchemeHttps.Equals(parsed.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Webhook URL must use HTTPS when EnforceHttps is enabled.", nameof(url));
            }
        }

        var request = new SetWebhookRequest
        {
            Url = url,
            UpdateTypes = updateTypes,
            Secret = secret
        };

        _options.Webhook.Endpoint = url;
        return await Subscriptions.SetWebhookAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the webhook subscription by calling <c>DELETE /subscriptions</c>.
    /// </summary>
    /// <param name="url">The webhook URL to remove from subscriptions.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with success status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when url is null or empty.</exception>
    public Task<Response> DeleteWebhookAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return Subscriptions.DeleteWebhookAsync(
            new DeleteWebhookRequest { Url = url },
            cancellationToken);
    }
}



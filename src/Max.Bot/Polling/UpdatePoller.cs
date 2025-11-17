using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Exceptions;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Max.Bot.Types.Requests;
using Microsoft.Extensions.Logging;

namespace Max.Bot.Polling;

/// <summary>
/// Implements resilient long polling against the MAX Bot API.
/// </summary>
public sealed class UpdatePoller : IAsyncDisposable
{
    private readonly IMaxBotApi _api;
    private readonly ISubscriptionsApi _subscriptionsApi;
    private readonly MaxBotOptions _options;
    private readonly ILogger<UpdatePoller>? _logger;
    private readonly IServiceProvider? _serviceProvider;
    private readonly object _lifecycleLock = new();
    private readonly List<Task> _inFlightHandlers = new();
    private readonly HashSet<UpdateType>? _handlingTypeFilter;
    private readonly string[]? _typeQueryFilter;
    private readonly HashSet<string>? _allowedUsernames;

    private CancellationTokenSource? _cts;
    private Task? _pollingTask;
    private IUpdateHandler? _handler;
    private long? _marker;
    private int _consecutiveErrors;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePoller"/> class.
    /// </summary>
    public UpdatePoller(
        IMaxBotApi api,
        ISubscriptionsApi subscriptionsApi,
        MaxBotOptions options,
        ILogger<UpdatePoller>? logger = null,
        IServiceProvider? serviceProvider = null)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _subscriptionsApi = subscriptionsApi ?? throw new ArgumentNullException(nameof(subscriptionsApi));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _serviceProvider = serviceProvider;

        _marker = _options.Polling.InitialMarker;
        _handlingTypeFilter = UpdateFilterUtilities.BuildTypeFilter(options);
        _typeQueryFilter = BuildTypeQueryFilter(options);
        _allowedUsernames = UpdateFilterUtilities.BuildAllowedUsernames(options);
    }

    /// <summary>
    /// Starts the polling loop.
    /// </summary>
    public Task StartAsync(IUpdateHandler handler, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lifecycleLock)
        {
            if (_pollingTask is { IsCompleted: false })
            {
                throw new InvalidOperationException("UpdatePoller is already running.");
            }

            _handler = handler;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _pollingTask = Task.Run(() => PollAsync(_cts.Token), CancellationToken.None);
        }

        _logger?.LogInformation(
            "UpdatePoller started with timeout {TimeoutSeconds}s and limit {Limit}.",
            _options.Polling.LongPollingTimeout.TotalSeconds,
            _options.Polling.BatchSize);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the polling loop and waits for in-flight handlers to complete.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task? runningTask = null;

        lock (_lifecycleLock)
        {
            if (_cts == null)
            {
                return;
            }

            _cts.Cancel();
            runningTask = _pollingTask;
        }

        if (runningTask != null)
        {
            try
            {
                await runningTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                // * Poller cancellation requested: swallow to ensure graceful shutdown
            }
        }

        await DrainHandlersAsync(cancellationToken).ConfigureAwait(false);

        lock (_lifecycleLock)
        {
            _cts?.Dispose();
            _cts = null;
            _pollingTask = null;
            _handler = null;
        }

        _logger?.LogInformation("UpdatePoller stopped.");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await FetchUpdatesAsync(_marker, cancellationToken).ConfigureAwait(false);
                _consecutiveErrors = 0;

                if (response.Updates?.Length > 0)
                {
                    foreach (var update in response.Updates)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (!ShouldDispatch(update))
                        {
                            continue;
                        }

                        await DispatchAsync(update, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    await Task.Delay(_options.Polling.IdleDelay, cancellationToken).ConfigureAwait(false);
                }

                if (_options.Polling.PersistMarkers)
                {
                    _marker = response.Marker ?? _marker;
                }
                else
                {
                    _marker = response.Marker ?? _options.Polling.InitialMarker;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (MaxRateLimitException ex)
            {
                await HandleTransientErrorAsync(ex, cancellationToken, "Rate limit reached while polling updates.").ConfigureAwait(false);
            }
            catch (MaxNetworkException ex)
            {
                await HandleTransientErrorAsync(ex, cancellationToken, "Network error while polling updates.").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleTransientErrorAsync(ex, cancellationToken, "Unexpected error in UpdatePoller.").ConfigureAwait(false);
            }
        }

        await DrainHandlersAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<GetUpdatesResponse> FetchUpdatesAsync(long? marker, CancellationToken cancellationToken)
    {
        var request = new GetUpdatesRequest
        {
            Limit = _options.Polling.BatchSize,
            Timeout = (int)Math.Round(_options.Polling.LongPollingTimeout.TotalSeconds),
            Marker = marker,
            Types = _typeQueryFilter != null ? new List<string>(_typeQueryFilter) : null
        };

        return await _subscriptionsApi.GetUpdatesAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private bool ShouldDispatch(Update update)
    {
        return UpdateFilterUtilities.ShouldDispatch(update, _handlingTypeFilter, _allowedUsernames);
    }

    private async Task DispatchAsync(Update update, CancellationToken cancellationToken)
    {
        if (_options.Handling.PreserveUpdateOrder || _options.Handling.MaxDegreeOfParallelism <= 1)
        {
            await HandleUpdateInternalAsync(update, cancellationToken).ConfigureAwait(false);
            return;
        }

        var handlerTask = HandleUpdateInternalAsync(update, cancellationToken);
        _inFlightHandlers.Add(handlerTask);

        if (_inFlightHandlers.Count >= _options.Handling.MaxDegreeOfParallelism)
        {
            var completed = await Task.WhenAny(_inFlightHandlers).ConfigureAwait(false);
            _inFlightHandlers.Remove(completed);
            await completed.ConfigureAwait(false);
        }

        _inFlightHandlers.RemoveAll(static task => task.IsCompleted);
    }

    private Task HandleUpdateInternalAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = _handler ?? throw new InvalidOperationException("UpdatePoller has not been started.");
        return UpdateHandlerExecutor.ExecuteAsync(update, handler, _options, _api, _logger, _serviceProvider, cancellationToken);
    }

    private async Task HandleTransientErrorAsync(Exception exception, CancellationToken cancellationToken, string message)
    {
        _consecutiveErrors++;
        var delay = NextBackoffDelay();
        _logger?.LogWarning(exception, "{Message} Retrying in {Delay}ms.", message, delay.TotalMilliseconds);
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
    }

    private TimeSpan NextBackoffDelay()
    {
        var backoffBase = _options.Polling.ErrorBackoffBase.TotalMilliseconds;
        var capped = Math.Min(
            backoffBase * Math.Pow(2, Math.Min(_consecutiveErrors, 10)),
            _options.Polling.ErrorBackoffMax.TotalMilliseconds);
        var jitter = 1 + ((Random.Shared.NextDouble() - 0.5) * 0.2); // +/-10%
        var delayMs = Math.Max(backoffBase, capped * jitter);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    private async Task DrainHandlersAsync(CancellationToken cancellationToken)
    {
        if (_inFlightHandlers.Count == 0)
        {
            return;
        }

        var handlers = _inFlightHandlers.ToArray();
        _inFlightHandlers.Clear();
        await Task.WhenAll(handlers.Select(task => task.WaitAsync(cancellationToken))).ConfigureAwait(false);
    }

    private static string[]? BuildTypeQueryFilter(MaxBotOptions options)
    {
        ICollection<UpdateType>? candidates = options.Polling.AllowedUpdateTypes is { Count: > 0 }
            ? options.Polling.AllowedUpdateTypes
            : options.Handling.AllowedUpdateTypes;

        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        return candidates
            .Select(value => ToSnakeCase(value.ToString()))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string ToSnakeCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 4);
        for (var index = 0; index < value.Length; index++)
        {
            var c = value[index];
            if (char.IsUpper(c))
            {
                if (index > 0)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}




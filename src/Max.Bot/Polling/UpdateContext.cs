using System;
using System.Collections.Generic;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Types;
using Microsoft.Extensions.Logging;

namespace Max.Bot.Polling;

/// <summary>
/// Represents the immutable context that accompanies each update dispatch.
/// </summary>
public sealed class UpdateContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateContext"/> class.
    /// </summary>
    /// <param name="update">The update payload supplied by the MAX API.</param>
    /// <param name="api">The API surface (typically <see cref="Max.Bot.MaxClient"/>) for executing follow-up calls.</param>
    /// <param name="options">Snapshot of <see cref="MaxBotOptions"/> used when the poller/webhook was created.</param>
    /// <param name="logger">Optional logger scoped to the poller or webhook pipeline.</param>
    /// <param name="services">Optional service provider for resolving user dependencies.</param>
    public UpdateContext(Update update, IMaxBotApi api, MaxBotOptions options, ILogger? logger = null, IServiceProvider? services = null)
    {
        Update = update ?? throw new ArgumentNullException(nameof(update));
        Api = api ?? throw new ArgumentNullException(nameof(api));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Logger = logger;
        Services = services;
        ReceivedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the update.
    /// </summary>
    public Update Update { get; }

    /// <summary>
    /// Gets the API surface for follow-up calls inside handlers.
    /// </summary>
    public IMaxBotApi Api { get; }

    /// <summary>
    /// Gets the options snapshot.
    /// </summary>
    public MaxBotOptions Options { get; }

    /// <summary>
    /// Gets the logger scoped to the dispatcher.
    /// </summary>
    public ILogger? Logger { get; }

    /// <summary>
    /// Gets the service provider (if the host application supplied one).
    /// </summary>
    public IServiceProvider? Services { get; }

    /// <summary>
    /// Gets the UTC timestamp recorded when the update entered the pipeline.
    /// </summary>
    public DateTimeOffset ReceivedAt { get; }

    /// <summary>
    /// Gets the bag for storing arbitrary data during handler execution.
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}







using System;
using System.Collections.Generic;
using Max.Bot.Types.Enums;

namespace Max.Bot.Configuration;

/// <summary>
/// Configures the long polling behaviour for <c>GET /updates</c> as described in
/// https://dev.max.ru/docs-api/methods/GET/updates.
/// </summary>
public sealed class MaxPollingOptions
{
    private const int MinLimit = 1;
    private const int MaxLimit = 1000;
    private const int MinTimeoutSeconds = 0;
    private const int MaxTimeoutSeconds = 90;

    /// <summary>
    /// Gets or sets the number of updates requested per HTTP call (maps to the <c>limit</c> query parameter).
    /// </summary>
    /// <remarks>
    /// The official MAX documentation limits the value to 1..1000. Telegram/Vk best practices suggest keeping it at 100
    /// to balance throughput and responsiveness.
    /// </remarks>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the long polling timeout (maps to the <c>timeout</c> query parameter).
    /// </summary>
    /// <remarks>
    /// MAX allows values between 0 and 90 seconds; aligning with Telegram.Bot defaults, 30 seconds keeps
    /// connections fresh without overwhelming the gateway.
    /// </remarks>
    public TimeSpan LongPollingTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the idle delay applied when the server returns no updates before issuing the next request.
    /// </summary>
    /// <remarks>
    /// Introducing a short delay prevents hammering the API when no events are available (recommended by VkNet maintainers).
    /// </remarks>
    public TimeSpan IdleDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the initial marker value sent to the API when polling starts.
    /// </summary>
    /// <remarks>
    /// When null, the poller resumes from the marker supplied by the server. Provide a value to replay missed updates (see MAX docs).
    /// </remarks>
    public long? InitialMarker { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the poller should persist the last marker returned by the API.
    /// </summary>
    /// <remarks>
    /// Persistence enables at-least-once delivery semantics similar to Telegram.Bot's offset handling.
    /// </remarks>
    public bool PersistMarkers { get; set; } = true;

    /// <summary>
    /// Gets or sets the allowed update types filter applied before dispatching to handlers.
    /// </summary>
    /// <remarks>
    /// Leaving the collection empty disables filtering; otherwise only listed <see cref="UpdateType"/> values will be requested via the API.
    /// </remarks>
    public ICollection<UpdateType> AllowedUpdateTypes { get; set; } = new List<UpdateType>();

    /// <summary>
    /// Gets or sets the maximum number of simultaneous HTTP requests used by the poller.
    /// </summary>
    /// <remarks>
    /// The current implementation issues a single request at a time; values greater than 1 are reserved for future sharding scenarios.
    /// </remarks>
    public int MaxConcurrentRequests { get; set; } = 1;

    /// <summary>
    /// Gets or sets the base delay used after transient failures before the poller retries (exponential backoff).
    /// </summary>
    public TimeSpan ErrorBackoffBase { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay applied when exponential backoff reaches its cap.
    /// </summary>
    public TimeSpan ErrorBackoffMax { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Ensures option values stay within the constraints defined by MAX Bot API documentation.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when configuration violates API requirements.</exception>
    public void Validate()
    {
        if (BatchSize is < MinLimit or > MaxLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(BatchSize), BatchSize, $"BatchSize must be between {MinLimit} and {MaxLimit} per GET /updates.");
        }

        if (LongPollingTimeout < TimeSpan.FromSeconds(MinTimeoutSeconds) ||
            LongPollingTimeout > TimeSpan.FromSeconds(MaxTimeoutSeconds))
        {
            throw new ArgumentOutOfRangeException(nameof(LongPollingTimeout), LongPollingTimeout, $"LongPollingTimeout must be between {MinTimeoutSeconds} and {MaxTimeoutSeconds} seconds as per GET /updates.");
        }

        if (IdleDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(IdleDelay), IdleDelay, "IdleDelay cannot be negative.");
        }

        if (MaxConcurrentRequests < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxConcurrentRequests), MaxConcurrentRequests, "At least one concurrent request is required.");
        }

        if (AllowedUpdateTypes is null)
        {
            throw new ArgumentNullException(nameof(AllowedUpdateTypes), "AllowedUpdateTypes cannot be null. Use an empty collection to disable filtering.");
        }

        if (ErrorBackoffBase <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ErrorBackoffBase), ErrorBackoffBase, "ErrorBackoffBase must be greater than zero.");
        }

        if (ErrorBackoffMax < ErrorBackoffBase)
        {
            throw new ArgumentException("ErrorBackoffMax must be greater than or equal to ErrorBackoffBase.", nameof(ErrorBackoffMax));
        }
    }
}




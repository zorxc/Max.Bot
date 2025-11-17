using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot;
using Max.Bot.Api;
using Max.Bot.Configuration;
using Max.Bot.Polling;
using Max.Bot.Types;

namespace Max.Bot.Examples;

/// <summary>
/// Abstraction used by samples to interact with the Max.Bot runtime.
/// </summary>
public interface ISampleRuntime : IAsyncDisposable
{
    /// <summary>
    /// Gets the API surface used by the sample.
    /// </summary>
    IMaxBotApi Api { get; }

    /// <summary>
    /// Starts the polling loop with the supplied update handler.
    /// </summary>
    Task StartPollingAsync(IUpdateHandler handler, CancellationToken cancellationToken);

    /// <summary>
    /// Stops the polling loop.
    /// </summary>
    Task StopPollingAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Default runtime backed by <see cref="MaxClient"/>.
/// </summary>
public sealed class MaxBotSampleRuntime : ISampleRuntime
{
    private readonly MaxClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxBotSampleRuntime"/> class.
    /// </summary>
    /// <param name="settings">The sample settings containing the bot token.</param>
    public MaxBotSampleRuntime(SampleSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var options = new MaxBotOptions
        {
            Token = settings.Token
        };

        options.Polling.AllowedUpdateTypes = new List<Types.Enums.UpdateType>
        {
            Types.Enums.UpdateType.Message,
            Types.Enums.UpdateType.CallbackQuery
        };

        _client = new MaxClient(options);
    }

    /// <inheritdoc />
    public IMaxBotApi Api => _client;

    /// <inheritdoc />
    public Task StartPollingAsync(IUpdateHandler handler, CancellationToken cancellationToken)
    {
        return _client.StartPollingAsync(handler, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task StopPollingAsync(CancellationToken cancellationToken)
    {
        return _client.StopPollingAsync(cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Execution context passed to each sample.
/// </summary>
public sealed class SampleExecutionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SampleExecutionContext"/> class.
    /// </summary>
    public SampleExecutionContext(ISampleRuntime runtime, SampleSettings settings, TextWriter? output = null, TextWriter? error = null)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Output = output ?? Console.Out;
        Error = error ?? Console.Error;
    }

    /// <summary>
    /// Gets the runtime abstraction.
    /// </summary>
    public ISampleRuntime Runtime { get; }

    /// <summary>
    /// Gets the strongly typed API surface.
    /// </summary>
    public IMaxBotApi Api => Runtime.Api;

    /// <summary>
    /// Gets the settings.
    /// </summary>
    public SampleSettings Settings { get; }

    /// <summary>
    /// Gets the output writer.
    /// </summary>
    public TextWriter Output { get; }

    /// <summary>
    /// Gets the error writer.
    /// </summary>
    public TextWriter Error { get; }
}

/// <summary>
/// Helper utilities shared across samples.
/// </summary>
public static class SampleUtilities
{
    /// <summary>
    /// Runs polling until cancellation and ensures proper shutdown.
    /// </summary>
    public static async Task RunPollingLoopAsync(SampleExecutionContext context, IUpdateHandler handler, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        await context.Runtime.StartPollingAsync(handler, cancellationToken).ConfigureAwait(false);
        context.Output.WriteLine("Polling started. Press Ctrl+C to stop.");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // * Cancellation requested by the host application.
        }
        finally
        {
            await context.Runtime.StopPollingAsync(CancellationToken.None).ConfigureAwait(false);
            context.Output.WriteLine("Polling stopped.");
        }
    }

    /// <summary>
    /// Extracts the normalized text body from an update.
    /// </summary>
    public static string GetNormalizedText(Types.Message? message)
    {
        var text = message?.Body?.Text ?? message?.Text;
        return text?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets the chat identifier from the supplied message.
    /// </summary>
    public static long? GetChatId(Types.Message? message)
    {
        return message?.Chat?.Id;
    }
}




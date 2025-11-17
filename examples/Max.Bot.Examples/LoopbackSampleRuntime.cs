using System;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Api;
using Max.Bot.Polling;

namespace Max.Bot.Examples;

/// <summary>
/// Provides an <see cref="ISampleRuntime"/> implementation that never touches the network.
/// </summary>
public sealed class LoopbackSampleRuntime : ISampleRuntime
{
    private readonly TaskCompletionSource<IUpdateHandler> _handlerSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopbackSampleRuntime"/> class.
    /// </summary>
    /// <param name="api">The mocked API surface exposed to samples.</param>
    public LoopbackSampleRuntime(IMaxBotApi api)
    {
        Api = api ?? throw new ArgumentNullException(nameof(api));
    }

    /// <inheritdoc />
    public IMaxBotApi Api { get; }

    /// <inheritdoc />
    public Task StartPollingAsync(IUpdateHandler handler, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlerSource.TrySetResult(handler);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopPollingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Waits for the handler registered by a sample during initialization.
    /// </summary>
    public Task<IUpdateHandler> WaitForHandlerAsync() => _handlerSource.Task;
}




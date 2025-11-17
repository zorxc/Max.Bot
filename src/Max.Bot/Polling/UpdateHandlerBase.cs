using System.Threading;
using System.Threading.Tasks;

namespace Max.Bot.Polling;

/// <summary>
/// Convenience base class mirroring Telegram.Bot's <c>IUpdateHandler</c> defaults.
/// </summary>
public abstract class UpdateHandlerBase : IUpdateHandler
{
    /// <inheritdoc />
    public virtual Task HandleUpdateAsync(UpdateContext context, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task HandleMessageAsync(UpdateContext context, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task HandleCallbackQueryAsync(UpdateContext context, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task HandleUnknownUpdateAsync(UpdateContext context, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}





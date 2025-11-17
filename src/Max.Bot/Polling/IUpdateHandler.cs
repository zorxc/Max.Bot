using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Types.Enums;

namespace Max.Bot.Polling;

/// <summary>
/// Contract for handling updates emitted by the poller or webhook pipelines.
/// </summary>
public interface IUpdateHandler
{
    /// <summary>
    /// Entry point invoked for every update before type-specific dispatch occurs.
    /// </summary>
    Task HandleUpdateAsync(UpdateContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Invoked for <see cref="UpdateType.Message"/> events.
    /// </summary>
    Task HandleMessageAsync(UpdateContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Invoked for <see cref="UpdateType.CallbackQuery"/> events.
    /// </summary>
    Task HandleCallbackQueryAsync(UpdateContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Invoked when an update type is not explicitly handled.
    /// </summary>
    Task HandleUnknownUpdateAsync(UpdateContext context, CancellationToken cancellationToken);
}





using System;
using System.Threading;
using System.Threading.Tasks;
using Max.Bot.Types;

namespace Max.Bot.Polling;

/// <summary>
/// Defines operations required to dispatch updates to application handlers.
/// </summary>
public interface IUpdatePipeline
{
    /// <summary>
    /// Processes a webhook update payload.
    /// </summary>
    Task ProcessWebhookAsync(Update update, IUpdateHandler handler, IServiceProvider? services = null, CancellationToken cancellationToken = default);
}







using System.Threading;
using System.Threading.Tasks;

namespace Max.Bot.Examples.Samples;

/// <summary>
/// Represents a runnable bot sample.
/// </summary>
public interface IBotSample
{
    /// <summary>
    /// Gets the human-friendly sample name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the sample description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the sample logic.
    /// </summary>
    Task RunAsync(SampleExecutionContext context, CancellationToken cancellationToken);
}




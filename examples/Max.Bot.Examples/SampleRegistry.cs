using System;
using System.Collections.Generic;
using Max.Bot.Examples.Samples;

namespace Max.Bot.Examples;

/// <summary>
/// Provides lookup helpers for available bot samples.
/// </summary>
public static class SampleRegistry
{
    private static readonly IReadOnlyDictionary<string, IBotSample> Samples = new Dictionary<string, IBotSample>(StringComparer.OrdinalIgnoreCase)
    {
        ["echo"] = new EchoBotSample(),
        ["commands"] = new CommandBotSample(),
        ["keyboard"] = new KeyboardBotSample(),
        ["files"] = new FileBotSample()
    };

    /// <summary>
    /// Gets the sample names available for execution.
    /// </summary>
    public static IEnumerable<string> AvailableSamples => Samples.Keys;

    /// <summary>
    /// Attempts to resolve a sample by name.
    /// </summary>
    public static bool TryGet(string? name, out IBotSample sample)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "echo";
        }

        return Samples.TryGetValue(name, out sample!);
    }
}



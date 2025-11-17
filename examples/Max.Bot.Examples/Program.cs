using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Max.Bot.Examples;

internal static class Program
{
    private static readonly string SampleList = string.Join(", ", SampleRegistry.AvailableSamples);

    public static async Task<int> Main(string[] args)
    {
        var sampleName = args.FirstOrDefault();
        if (!SampleRegistry.TryGet(sampleName, out var sample))
        {
            Console.Error.WriteLine($"Unknown sample '{sampleName}'. Available options: {SampleList}");
            return 1;
        }

        SampleSettings settings;
        try
        {
            settings = SampleSettings.LoadFromEnvironment();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        await using var runtime = new MaxBotSampleRuntime(settings);
        var context = new SampleExecutionContext(runtime, settings);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine($"Running '{sample.Name}' sample. Press Ctrl+C to exit.");
        await sample.RunAsync(context, cts.Token).ConfigureAwait(false);
        return 0;
    }
}



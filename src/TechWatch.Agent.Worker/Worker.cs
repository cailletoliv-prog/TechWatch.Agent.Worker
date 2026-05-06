using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Scheduling;

namespace TechWatch.Agent.Worker
{
    public class Worker(
        TechWatchPipeline pipeline,
        IOptions<TechWatchOptions> options,
        IHostApplicationLifetime applicationLifetime,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!options.Value.RunOnce)
            {
                logger.LogInformation("RunOnce is disabled; running one pipeline pass because no scheduler is configured yet");
            }

            await pipeline.RunAsync(stoppingToken);

            if (options.Value.RunOnce)
            {
                logger.LogInformation("RunOnce completed; stopping application");
                applicationLifetime.StopApplication();
            }
        }
    }
}

using TechWatch.Agent.Worker.Scheduling;

namespace TechWatch.Agent.Worker
{
    public class Worker(TechWatchPipeline pipeline) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return pipeline.RunAsync(stoppingToken);
        }
    }
}

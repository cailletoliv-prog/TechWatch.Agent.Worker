namespace TechWatch.Agent.Worker.Scheduling;

public sealed class TechWatchPipeline(ILogger<TechWatchPipeline> logger)
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("pipeline started");

        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("pipeline completed");

        return Task.CompletedTask;
    }
}

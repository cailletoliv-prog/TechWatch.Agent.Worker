using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker.Sources;

namespace TechWatch.Agent.Worker.Scheduling;

public sealed class TechWatchPipeline(
    ISourceAggregator sourceAggregator,
    IContentFilter contentFilter,
    ILogger<TechWatchPipeline> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("pipeline started");

        var items = await sourceAggregator.FetchAsync(cancellationToken);
        if (items.Count == 0)
        {
            logger.LogInformation("pipeline completed: no items fetched");
            return;
        }

        var relevantCount = 0;
        var rejectedCount = 0;

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = contentFilter.Evaluate(item);
            if (result.IsRelevant)
            {
                relevantCount++;
                logger.LogInformation(
                    "relevant item detected: score {Score}; title {Title}; source {SourceName}; url {Url}; reason {Reason}",
                    result.Score,
                    item.Title,
                    item.SourceName,
                    item.Url,
                    result.Reason);

                continue;
            }

            rejectedCount++;
        }

        logger.LogInformation(
            "pipeline completed: total items {TotalItemCount}, relevant items {RelevantItemCount}, rejected items {RejectedItemCount}",
            items.Count,
            relevantCount,
            rejectedCount);
    }
}

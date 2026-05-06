using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker.Models;
using TechWatch.Agent.Worker.Sources;
using TechWatch.Agent.Worker.Storage;

namespace TechWatch.Agent.Worker.Scheduling;

public sealed class TechWatchPipeline(
    ISourceAggregator sourceAggregator,
    IContentFilter contentFilter,
    ITechItemRepository techItemRepository,
    ILogger<TechWatchPipeline> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("pipeline started");

        await techItemRepository.InitializeAsync(cancellationToken);

        var items = await sourceAggregator.FetchAsync(cancellationToken);
        if (items.Count == 0)
        {
            logger.LogInformation("pipeline completed: no items fetched");
            return;
        }

        var relevantCount = 0;
        var rejectedCount = 0;
        var storedCount = 0;
        var ignoredCount = 0;

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

                var pendingItem = MarkPendingAnalysis(item);
                if (await techItemRepository.UpsertAsync(pendingItem, cancellationToken))
                {
                    storedCount++;
                }
                else
                {
                    ignoredCount++;
                }

                continue;
            }

            rejectedCount++;
        }

        logger.LogInformation(
            "pipeline completed: total items {TotalItemCount}, relevant items {RelevantItemCount}, rejected items {RejectedItemCount}, stored items {StoredItemCount}, ignored items {IgnoredItemCount}",
            items.Count,
            relevantCount,
            rejectedCount,
            storedCount,
            ignoredCount);
    }

    private static TechItem MarkPendingAnalysis(TechItem item)
    {
        return new TechItem
        {
            Id = item.Id,
            SourceId = item.SourceId,
            SourceName = item.SourceName,
            SourceType = item.SourceType,
            Title = item.Title,
            Url = item.Url,
            Author = item.Author,
            Summary = item.Summary,
            Content = item.Content,
            PublishedAt = item.PublishedAt,
            FetchedAt = item.FetchedAt,
            Status = TechItemStatus.PendingAnalysis
        };
    }
}

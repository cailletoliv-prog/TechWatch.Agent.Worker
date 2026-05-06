using System.Diagnostics;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Digests;
using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker.Llm;
using TechWatch.Agent.Worker.Models;
using TechWatch.Agent.Worker.Sources;
using TechWatch.Agent.Worker.Storage;

namespace TechWatch.Agent.Worker.Scheduling;

public sealed class TechWatchPipeline(
    ISourceAggregator sourceAggregator,
    IContentFilter contentFilter,
    ITechItemRepository techItemRepository,
    IContentAnalyzer contentAnalyzer,
    IDigestGenerator digestGenerator,
    IOptions<OllamaOptions> ollamaOptions,
    ILogger<TechWatchPipeline> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("pipeline started");

        await techItemRepository.InitializeAsync(cancellationToken);

        var items = await sourceAggregator.FetchAsync(cancellationToken);
        logger.LogInformation("pipeline fetched items: {FetchedItemCount}", items.Count);

        if (items.Count == 0)
        {
            logger.LogInformation("pipeline completed: no items fetched");
            await AnalyzePendingItemsAsync(cancellationToken);
            await GenerateDigestAsync(cancellationToken);
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
            "filtering completed: fetched items {TotalItemCount}, relevant items {RelevantItemCount}, rejected items {RejectedItemCount}, stored items {StoredItemCount}, ignored items {IgnoredItemCount}",
            items.Count,
            relevantCount,
            rejectedCount,
            storedCount,
            ignoredCount);

        await AnalyzePendingItemsAsync(cancellationToken);
        await GenerateDigestAsync(cancellationToken);
    }

    private async Task AnalyzePendingItemsAsync(CancellationToken cancellationToken)
    {
        var pendingItems = await techItemRepository.GetPendingAnalysisAsync(
            ollamaOptions.Value.MaxItemsPerRun,
            cancellationToken);

        if (pendingItems.Count == 0)
        {
            logger.LogInformation("analysis completed: no pending items");
            return;
        }

        var analyzedCount = 0;
        var failedCount = 0;

        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var analysis = await contentAnalyzer.AnalyzeAsync(item, cancellationToken);
                await techItemRepository.SaveAnalysisAsync(analysis, cancellationToken);
                analyzedCount++;

                logger.LogInformation(
                    "analysis completed for item: score {Score}; title {Title}; source {SourceName}; url {Url}",
                    analysis.InterestScore,
                    item.Title,
                    item.SourceName,
                    item.Url);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedCount++;
                await techItemRepository.MarkAnalysisFailedAsync(item.Id, cancellationToken);
                logger.LogWarning(
                    exception,
                    "analysis failed for item: title {Title}; source {SourceName}; url {Url}",
                    item.Title,
                    item.SourceName,
                    item.Url);
            }
        }

        logger.LogInformation(
            "analysis completed: analyzed items {AnalyzedItemCount}, failed items {FailedItemCount}",
            analyzedCount,
            failedCount);
    }

    private async Task GenerateDigestAsync(CancellationToken cancellationToken)
    {
        var runDate = DateTimeOffset.UtcNow;
        var since = runDate.Date;
        var entries = await techItemRepository.GetRecentAnalysisResultsAsync(since, cancellationToken);
        var digestRun = await digestGenerator.GenerateAsync(entries, runDate, cancellationToken);

        logger.LogInformation(
            "digest generated: entries {EntryCount}; output {OutputPath}",
            digestRun.Entries.Count,
            digestRun.OutputPath);

        OpenDigest(digestRun.OutputPath);
    }

    private void OpenDigest(string outputPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(outputPath) || !File.Exists(outputPath))
            {
                logger.LogWarning("digest could not be opened because the file does not exist: {OutputPath}", outputPath);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "digest could not be opened: {OutputPath}",
                outputPath);
        }
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

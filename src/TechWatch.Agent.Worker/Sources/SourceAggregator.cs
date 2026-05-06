using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Sources;

public sealed class SourceAggregator(
    IOptions<TechWatchOptions> options,
    IEnumerable<ISourceReader> readers,
    ILogger<SourceAggregator> logger) : ISourceAggregator
{
    private readonly IReadOnlyCollection<ISourceReader> readers = readers.ToArray();

    public async Task<IReadOnlyCollection<TechItem>> FetchAsync(
        IReadOnlyCollection<SourceDefinition> sources,
        CancellationToken cancellationToken)
    {
        var items = new List<TechItem>();

        foreach (var source in sources.Where(source => source.Enabled))
        {
            var reader = readers.FirstOrDefault(reader => reader.SourceType == source.Type);
            if (reader is null)
            {
                logger.LogWarning("No source reader registered for source type {SourceType}", source.Type);
                continue;
            }

            try
            {
                items.AddRange(await reader.ReadAsync(source, cancellationToken));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Failed to read source {SourceName} from {SourceUrl}",
                    source.Name,
                    source.Url);
            }
        }

        return items;
    }

    public Task<IReadOnlyCollection<TechItem>> FetchAsync(CancellationToken cancellationToken)
    {
        var sources = options.Value.Sources
            .Where(source => source.Enabled)
            .Select(ToSourceDefinition)
            .Where(source => source is not null)
            .Select(source => source!)
            .ToArray();

        return FetchAsync(sources, cancellationToken);
    }

    private static SourceDefinition? ToSourceDefinition(SourceOptions source)
    {
        var sourceType = ParseSourceType(source.Type);
        if (sourceType is null)
        {
            return null;
        }

        return new SourceDefinition
        {
            Name = source.Name,
            Type = sourceType.Value,
            Url = source.Url,
            Enabled = source.Enabled
        };
    }

    private static SourceType? ParseSourceType(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "rss" => SourceType.Rss,
            "atom" => SourceType.Rss,
            "blog" => SourceType.Blog,
            "github-releases" => SourceType.GitHubReleases,
            _ => null
        };
    }
}

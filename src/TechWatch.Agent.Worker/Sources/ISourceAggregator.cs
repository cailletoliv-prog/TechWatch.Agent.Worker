using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Sources;

public interface ISourceAggregator
{
    Task<IReadOnlyCollection<TechItem>> FetchAsync(
        IReadOnlyCollection<SourceDefinition> sources,
        CancellationToken cancellationToken);
}

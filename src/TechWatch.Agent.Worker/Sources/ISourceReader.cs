using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Sources;

public interface ISourceReader
{
    SourceType SourceType { get; }

    Task<IReadOnlyCollection<TechItem>> ReadAsync(
        SourceDefinition source,
        CancellationToken cancellationToken);
}

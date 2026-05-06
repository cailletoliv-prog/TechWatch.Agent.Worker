using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Digests;

public interface IDigestGenerator
{
    Task<DigestRun> GenerateAsync(
        IReadOnlyCollection<DigestEntry> entries,
        DateTimeOffset runDate,
        CancellationToken cancellationToken);
}

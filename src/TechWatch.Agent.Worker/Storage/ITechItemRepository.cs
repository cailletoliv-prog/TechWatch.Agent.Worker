using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Storage;

public interface ITechItemRepository
{
    Task SaveItemAsync(
        TechItem item,
        CancellationToken cancellationToken);

    Task SaveAnalysisAsync(
        AnalysisResult analysisResult,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DigestEntry>> GetDigestEntriesAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken);
}

using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Storage;

public interface ITechItemRepository
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task<bool> UpsertAsync(
        TechItem item,
        CancellationToken cancellationToken);

    Task<bool> ExistsByUrlAsync(
        string url,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TechItem>> GetPendingAnalysisAsync(
        int limit,
        CancellationToken cancellationToken);

    Task SaveAnalysisAsync(
        AnalysisResult analysisResult,
        CancellationToken cancellationToken);

    Task MarkAnalysisFailedAsync(
        Guid techItemId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DigestEntry>> GetRecentAnalysisResultsAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken);
}

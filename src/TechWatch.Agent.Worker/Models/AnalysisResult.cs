namespace TechWatch.Agent.Worker.Models;

public sealed class AnalysisResult
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid TechItemId { get; init; }

    public int InterestScore { get; init; }

    public string Summary { get; init; } = string.Empty;

    public bool HasBreakingChange { get; init; }

    public string Importance { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Tags { get; init; } = [];

    public DateTimeOffset AnalyzedAt { get; init; } = DateTimeOffset.UtcNow;
}

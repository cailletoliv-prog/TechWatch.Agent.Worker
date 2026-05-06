namespace TechWatch.Agent.Worker.Models;

public sealed class DigestEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid TechItemId { get; init; }

    public Guid AnalysisResultId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string SourceName { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public int InterestScore { get; init; }

    public bool HasBreakingChange { get; init; }

    public DateTimeOffset PublishedAt { get; init; }
}

namespace TechWatch.Agent.Worker.Models;

public sealed class DigestRun
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset RunDate { get; init; }

    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;

    public string OutputPath { get; init; } = string.Empty;

    public IReadOnlyCollection<DigestEntry> Entries { get; init; } = [];
}

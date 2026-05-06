namespace TechWatch.Agent.Worker.Models;

public sealed class TechItem
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid SourceId { get; init; }

    public string SourceName { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string? Author { get; init; }

    public string? Summary { get; init; }

    public string? Content { get; init; }

    public DateTimeOffset PublishedAt { get; init; }

    public DateTimeOffset FetchedAt { get; init; } = DateTimeOffset.UtcNow;

    public TechItemStatus Status { get; init; } = TechItemStatus.New;
}

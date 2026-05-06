namespace TechWatch.Agent.Worker.Models;

public sealed class SourceDefinition
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; init; } = string.Empty;

    public SourceType Type { get; init; }

    public string Url { get; init; } = string.Empty;

    public bool Enabled { get; init; } = true;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

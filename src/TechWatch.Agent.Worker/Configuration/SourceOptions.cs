namespace TechWatch.Agent.Worker.Configuration;

public sealed class SourceOptions
{
    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public bool Enabled { get; init; } = true;
}

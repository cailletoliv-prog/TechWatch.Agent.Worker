namespace TechWatch.Agent.Worker.Configuration;

public sealed class TechWatchOptions
{
    public const string SectionName = "TechWatch";

    public bool RunOnce { get; init; } = true;

    public List<SourceOptions> Sources { get; init; } = [];

    public OllamaOptions Ollama { get; init; } = new();

    public FilterOptions Filtering { get; init; } = new();

    public DigestOptions Digest { get; init; } = new();

    public StorageOptions Storage { get; init; } = new();
}

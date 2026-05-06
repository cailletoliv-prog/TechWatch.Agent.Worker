namespace TechWatch.Agent.Worker.Configuration;

public sealed class OllamaOptions
{
    public string BaseUrl { get; init; } = "http://localhost:11434";

    public string Model { get; init; } = "llama3.1";

    public int TimeoutSeconds { get; init; } = 120;

    public int MaxItemsPerRun { get; init; } = 5;
}

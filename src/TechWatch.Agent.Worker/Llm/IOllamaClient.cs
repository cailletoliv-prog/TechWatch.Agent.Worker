namespace TechWatch.Agent.Worker.Llm;

public interface IOllamaClient
{
    Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken);
}

namespace TechWatch.Agent.Worker.Configuration;

public sealed class FilterOptions
{
    public List<string> PositiveKeywords { get; init; } = [];

    public List<string> NegativeKeywords { get; init; } = [];

    public int MinimumScore { get; init; } = 1;
}

namespace TechWatch.Agent.Worker.Configuration;

public sealed class FilterOptions
{
    public List<string> PositiveKeywords { get; init; } = [];

    public List<string> StrongPositiveKeywords { get; init; } = [];

    public List<string> WeakPositiveKeywords { get; init; } = [];

    public List<string> NegativeKeywords { get; init; } = [];

    public int MinimumScore { get; init; } = 1;

    public int PositiveKeywordWeight { get; init; } = 1;

    public int StrongKeywordWeight { get; init; } = 3;

    public int WeakKeywordWeight { get; init; } = 0;

    public int WeakKeywordPenalty { get; init; } = 1;
}

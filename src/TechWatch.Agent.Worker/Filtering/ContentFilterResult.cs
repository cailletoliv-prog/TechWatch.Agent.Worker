namespace TechWatch.Agent.Worker.Filtering;

public sealed class ContentFilterResult
{
    public bool IsRelevant { get; init; }

    public int Score { get; init; }

    public IReadOnlyCollection<string> MatchedPositiveKeywords { get; init; } = [];

    public IReadOnlyCollection<string> MatchedNegativeKeywords { get; init; } = [];

    public string Reason { get; init; } = string.Empty;
}

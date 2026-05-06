using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Filtering;

public sealed class KeywordContentFilter(IOptions<FilterOptions> options) : IContentFilter
{
    private readonly FilterOptions options = options.Value;

    public ContentFilterResult Evaluate(TechItem item)
    {
        var searchableText = string.Join(
            ' ',
            item.Title,
            item.Summary,
            item.Content);

        var positiveMatches = FindMatches(searchableText, options.PositiveKeywords);
        var negativeMatches = FindMatches(searchableText, options.NegativeKeywords);
        var score = positiveMatches.Count;

        if (negativeMatches.Count > 0)
        {
            return new ContentFilterResult
            {
                IsRelevant = false,
                Score = score,
                MatchedPositiveKeywords = positiveMatches,
                MatchedNegativeKeywords = negativeMatches,
                Reason = "Rejected because negative keywords matched."
            };
        }

        if (score < options.MinimumScore)
        {
            return new ContentFilterResult
            {
                IsRelevant = false,
                Score = score,
                MatchedPositiveKeywords = positiveMatches,
                MatchedNegativeKeywords = negativeMatches,
                Reason = "Rejected because the score is below the minimum."
            };
        }

        return new ContentFilterResult
        {
            IsRelevant = true,
            Score = score,
            MatchedPositiveKeywords = positiveMatches,
            MatchedNegativeKeywords = negativeMatches,
            Reason = "Accepted because positive keyword score meets the minimum."
        };
    }

    private static IReadOnlyCollection<string> FindMatches(
        string text,
        IEnumerable<string> keywords)
    {
        return keywords
            .Where(keyword => !string.IsNullOrWhiteSpace(keyword))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}

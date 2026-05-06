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
        var strongMatches = FindMatches(searchableText, options.StrongPositiveKeywords);
        var weakMatches = FindMatches(searchableText, options.WeakPositiveKeywords);
        var negativeMatches = FindMatches(searchableText, options.NegativeKeywords);
        var matchedPositiveKeywords = positiveMatches
            .Concat(strongMatches)
            .Concat(weakMatches)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var score =
            positiveMatches.Count * options.PositiveKeywordWeight
            + strongMatches.Count * options.StrongKeywordWeight
            + weakMatches.Count * options.WeakKeywordWeight;

        if (weakMatches.Count > 0 && strongMatches.Count == 0)
        {
            score -= options.WeakKeywordPenalty;
        }

        score = Math.Max(0, score);

        if (negativeMatches.Count > 0)
        {
            return new ContentFilterResult
            {
                IsRelevant = false,
                Score = score,
                MatchedPositiveKeywords = matchedPositiveKeywords,
                MatchedNegativeKeywords = negativeMatches,
                Reason = "Rejected because negative keywords matched."
            };
        }

        if (weakMatches.Count > 0 && strongMatches.Count == 0 && score < options.MinimumScore)
        {
            return new ContentFilterResult
            {
                IsRelevant = false,
                Score = score,
                MatchedPositiveKeywords = matchedPositiveKeywords,
                MatchedNegativeKeywords = negativeMatches,
                Reason = "Rejected because weak keywords matched without a strong technical signal."
            };
        }

        if (score < options.MinimumScore)
        {
            return new ContentFilterResult
            {
                IsRelevant = false,
                Score = score,
                MatchedPositiveKeywords = matchedPositiveKeywords,
                MatchedNegativeKeywords = negativeMatches,
                Reason = "Rejected because the score is below the minimum."
            };
        }

        return new ContentFilterResult
        {
            IsRelevant = true,
            Score = score,
            MatchedPositiveKeywords = matchedPositiveKeywords,
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

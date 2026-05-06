using FluentAssertions;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Filtering;

public sealed class KeywordContentFilterTests
{
    [Fact]
    public void Accepts_relevant_dotnet_item()
    {
        var filter = CreateFilter();
        var item = CreateItem(
            title: ".NET performance improvements",
            summary: "ASP.NET Core and EF Core updates for backend developers.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeTrue();
        result.Score.Should().Be(3);
        result.MatchedPositiveKeywords.Should().BeEquivalentTo(".NET", "ASP.NET Core", "EF Core");
        result.MatchedNegativeKeywords.Should().BeEmpty();
    }

    [Fact]
    public void Accepts_relevant_angular_item()
    {
        var filter = CreateFilter();
        var item = CreateItem(
            title: "Angular signals guide",
            content: "TypeScript examples for frontend development.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeTrue();
        result.Score.Should().Be(2);
        result.MatchedPositiveKeywords.Should().BeEquivalentTo("Angular", "TypeScript");
    }

    [Fact]
    public void Rejects_marketing_crypto_item()
    {
        var filter = CreateFilter();
        var item = CreateItem(
            title: "Crypto marketing campaign",
            summary: "Sponsored blockchain content for developers.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeFalse();
        result.MatchedNegativeKeywords.Should().BeEquivalentTo("crypto", "blockchain", "sponsored", "marketing");
        result.Reason.Should().Contain("negative keywords");
    }

    [Fact]
    public void Rejects_item_below_minimum_score()
    {
        var filter = CreateFilter(minimumScore: 2);
        var item = CreateItem(title: "C# tip of the day");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeFalse();
        result.Score.Should().Be(1);
        result.MatchedPositiveKeywords.Should().BeEquivalentTo("C#");
        result.Reason.Should().Contain("below the minimum");
    }

    [Fact]
    public void Matches_keywords_case_insensitively()
    {
        var filter = CreateFilter();
        var item = CreateItem(
            title: "angular release notes",
            summary: "typescript migration details.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeTrue();
        result.Score.Should().Be(2);
        result.MatchedPositiveKeywords.Should().BeEquivalentTo("Angular", "TypeScript");
    }

    private static KeywordContentFilter CreateFilter(int minimumScore = 1)
    {
        var options = new FilterOptions
        {
            PositiveKeywords =
            [
                ".NET",
                "C#",
                "ASP.NET Core",
                "EF Core",
                "Angular",
                "TypeScript"
            ],
            NegativeKeywords =
            [
                "crypto",
                "blockchain",
                "sponsored",
                "marketing"
            ],
            MinimumScore = minimumScore
        };

        return new KeywordContentFilter(Options.Create(options));
    }

    private static TechItem CreateItem(
        string title,
        string? summary = null,
        string? content = null)
    {
        return new TechItem
        {
            SourceId = Guid.NewGuid(),
            SourceName = "Test Source",
            Title = title,
            Url = "https://example.com/item",
            Summary = summary,
            Content = content,
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z")
        };
    }
}

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
        result.Score.Should().Be(7);
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

    [Fact]
    public void Strong_keyword_can_make_item_relevant()
    {
        var filter = CreateFilter(minimumScore: 2);
        var item = CreateItem(
            title: "MCP server security update",
            summary: "A security fix for AI agent tooling.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeTrue();
        result.Score.Should().BeGreaterThanOrEqualTo(6);
        result.MatchedPositiveKeywords.Should().Contain(["MCP", "security", "AI agent"]);
    }

    [Fact]
    public void Rejects_weak_demo_without_strong_signal()
    {
        var filter = CreateFilter(minimumScore: 2);
        var item = CreateItem(
            title: "Product demo showcase",
            summary: "A tutorial and demo with no concrete technical change.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeFalse();
        result.Score.Should().Be(0);
        result.MatchedPositiveKeywords.Should().BeEquivalentTo("demo", "showcase", "tutorial");
        result.Reason.Should().Contain("weak keywords");
    }

    [Fact]
    public void Penalizes_showcase_when_no_strong_keyword_matches()
    {
        var filter = CreateFilter(minimumScore: 2);
        var item = CreateItem(
            title: "Angular showcase demo",
            summary: "A preview tutorial for UI ideas.");

        var result = filter.Evaluate(item);

        result.IsRelevant.Should().BeFalse();
        result.Score.Should().Be(0);
        result.MatchedPositiveKeywords.Should().Contain(["Angular", "showcase", "demo", "preview", "tutorial"]);
    }

    private static KeywordContentFilter CreateFilter(int minimumScore = 1)
    {
        var options = new FilterOptions
        {
            PositiveKeywords =
            [
                ".NET",
                "C#",
                "Angular",
                "TypeScript"
            ],
            StrongPositiveKeywords =
            [
                "security",
                "breaking change",
                "EF Core",
                "ASP.NET Core",
                "AI agent",
                "MCP"
            ],
            WeakPositiveKeywords =
            [
                "preview",
                "showcase",
                "demo",
                "tutorial"
            ],
            NegativeKeywords =
            [
                "crypto",
                "blockchain",
                "sponsored",
                "marketing"
            ],
            MinimumScore = minimumScore,
            PositiveKeywordWeight = 1,
            StrongKeywordWeight = 3,
            WeakKeywordWeight = 0,
            WeakKeywordPenalty = 1
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

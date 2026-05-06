using FluentAssertions;
using NSubstitute;
using TechWatch.Agent.Worker.Llm;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Llm;

public sealed class OllamaContentAnalyzerTests
{
    [Fact]
    public async Task Analyze_async_parses_valid_json_response()
    {
        var item = CreateItem();
        var ollamaClient = Substitute.For<IOllamaClient>();
        ollamaClient
            .GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""
                {
                  "interestScore": 8,
                  "summary": "Important ASP.NET Core update.",
                  "importance": "High",
                  "isBreakingChange": true,
                  "tags": ["aspnetcore", "breaking-change"],
                  "reason": "Contains a breaking behavior change."
                }
                """);
        var analyzer = new OllamaContentAnalyzer(ollamaClient);

        var result = await analyzer.AnalyzeAsync(item, CancellationToken.None);

        result.TechItemId.Should().Be(item.Id);
        result.InterestScore.Should().Be(8);
        result.Summary.Should().Be("Important ASP.NET Core update.");
        result.Importance.Should().Be("High");
        result.HasBreakingChange.Should().BeTrue();
        result.Tags.Should().BeEquivalentTo("aspnetcore", "breaking-change");
        result.Reason.Should().Be("Contains a breaking behavior change.");
    }

    [Fact]
    public void Try_parse_accepts_text_around_json()
    {
        var techItemId = Guid.NewGuid();
        var response = """
            Sure, here is the JSON:
            {
              "interestScore": 6,
              "summary": "EF Core release notes.",
              "importance": "Medium",
              "isBreakingChange": false,
              "tags": ["efcore"],
              "reason": "Relevant release."
            }
            Done.
            """;

        var result = OllamaContentAnalyzer.TryParse(techItemId, response);

        result.Should().NotBeNull();
        result!.TechItemId.Should().Be(techItemId);
        result.InterestScore.Should().Be(6);
        result.Tags.Should().BeEquivalentTo("efcore");
    }

    [Fact]
    public async Task Analyze_async_returns_fallback_when_json_is_invalid()
    {
        var item = CreateItem();
        var ollamaClient = Substitute.For<IOllamaClient>();
        ollamaClient
            .GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("This is not JSON.");
        var analyzer = new OllamaContentAnalyzer(ollamaClient);

        var result = await analyzer.AnalyzeAsync(item, CancellationToken.None);

        result.TechItemId.Should().Be(item.Id);
        result.InterestScore.Should().Be(0);
        result.Importance.Should().Be("Unknown");
        result.Summary.Should().Be("Analysis unavailable.");
        result.Reason.Should().Contain("could not be parsed");
    }

    private static TechItem CreateItem()
    {
        return new TechItem
        {
            SourceId = Guid.NewGuid(),
            SourceName = "Test Source",
            SourceType = SourceType.Rss,
            Title = "ASP.NET Core release",
            Url = "https://example.com/aspnetcore",
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z"),
            Summary = "Release notes."
        };
    }
}

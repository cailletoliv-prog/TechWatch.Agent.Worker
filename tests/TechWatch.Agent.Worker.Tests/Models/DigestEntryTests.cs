using FluentAssertions;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Models;

public sealed class DigestEntryTests
{
    [Fact]
    public void Can_create_digest_entry_with_item_and_analysis_references()
    {
        var techItemId = Guid.NewGuid();
        var analysisResultId = Guid.NewGuid();

        var entry = new DigestEntry
        {
            TechItemId = techItemId,
            AnalysisResultId = analysisResultId,
            Title = "ASP.NET Core release",
            Url = "https://example.com/aspnetcore",
            SourceName = "GitHub Releases",
            Summary = "A release worth tracking.",
            InterestScore = 72,
            HasBreakingChange = false,
            PublishedAt = DateTimeOffset.Parse("2026-05-06T09:00:00Z")
        };

        entry.Id.Should().NotBeEmpty();
        entry.TechItemId.Should().Be(techItemId);
        entry.AnalysisResultId.Should().Be(analysisResultId);
        entry.InterestScore.Should().Be(72);
        entry.HasBreakingChange.Should().BeFalse();
    }
}

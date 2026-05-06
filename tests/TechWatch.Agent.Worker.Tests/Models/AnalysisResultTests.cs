using FluentAssertions;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Models;

public sealed class AnalysisResultTests
{
    [Fact]
    public void Can_create_analysis_result_with_expected_values()
    {
        var techItemId = Guid.NewGuid();

        var result = new AnalysisResult
        {
            TechItemId = techItemId,
            InterestScore = 85,
            Summary = "Important .NET update.",
            HasBreakingChange = true,
            Importance = "High",
            Tags = ["dotnet", "breaking-change"]
        };

        result.Id.Should().NotBeEmpty();
        result.TechItemId.Should().Be(techItemId);
        result.InterestScore.Should().Be(85);
        result.Summary.Should().Be("Important .NET update.");
        result.HasBreakingChange.Should().BeTrue();
        result.Tags.Should().ContainInOrder("dotnet", "breaking-change");
    }
}

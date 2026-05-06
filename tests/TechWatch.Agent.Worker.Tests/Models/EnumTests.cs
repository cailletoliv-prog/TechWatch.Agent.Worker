using FluentAssertions;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Models;

public sealed class EnumTests
{
    [Fact]
    public void Source_type_values_are_stable()
    {
        SourceType.Rss.Should().Be((SourceType)1);
        SourceType.GitHubReleases.Should().Be((SourceType)2);
        SourceType.Blog.Should().Be((SourceType)3);
    }

    [Fact]
    public void Tech_item_status_values_are_stable()
    {
        TechItemStatus.New.Should().Be((TechItemStatus)1);
        TechItemStatus.FilteredOut.Should().Be((TechItemStatus)2);
        TechItemStatus.PendingAnalysis.Should().Be((TechItemStatus)3);
        TechItemStatus.Analyzed.Should().Be((TechItemStatus)4);
        TechItemStatus.IncludedInDigest.Should().Be((TechItemStatus)5);
    }
}

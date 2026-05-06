using FluentAssertions;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Models;

public sealed class TechItemTests
{
    [Fact]
    public void Can_create_tech_item_with_minimal_values()
    {
        var sourceId = Guid.NewGuid();
        var publishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z");

        var item = new TechItem
        {
            SourceId = sourceId,
            SourceName = "Microsoft .NET Blog",
            Title = ".NET release notes",
            Url = "https://example.com/dotnet-release",
            PublishedAt = publishedAt
        };

        item.Id.Should().NotBeEmpty();
        item.SourceId.Should().Be(sourceId);
        item.Title.Should().Be(".NET release notes");
        item.Url.Should().Be("https://example.com/dotnet-release");
        item.PublishedAt.Should().Be(publishedAt);
        item.Status.Should().Be(TechItemStatus.New);
    }
}

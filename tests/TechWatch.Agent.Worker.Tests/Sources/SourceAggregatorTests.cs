using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Models;
using TechWatch.Agent.Worker.Sources;

namespace TechWatch.Agent.Worker.Tests.Sources;

public sealed class SourceAggregatorTests
{
    [Fact]
    public async Task Continues_when_one_source_fails()
    {
        var successfulSource = new SourceDefinition
        {
            Name = "Good feed",
            Type = SourceType.Rss,
            Url = "https://example.com/good.xml"
        };
        var failingSource = new SourceDefinition
        {
            Name = "Broken feed",
            Type = SourceType.Rss,
            Url = "https://example.com/broken.xml"
        };
        var expectedItem = new TechItem
        {
            SourceId = successfulSource.Id,
            SourceName = successfulSource.Name,
            SourceType = SourceType.Rss,
            Title = "Useful item",
            Url = "https://example.com/item",
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z")
        };
        var reader = new StubSourceReader(failingSource.Id, expectedItem);
        var aggregator = new SourceAggregator(
            Options.Create(new TechWatchOptions()),
            [reader],
            NullLogger<SourceAggregator>.Instance);

        var items = await aggregator.FetchAsync(
            [failingSource, successfulSource],
            CancellationToken.None);

        items.Should().ContainSingle().Which.Should().BeSameAs(expectedItem);
    }

    private sealed class StubSourceReader(Guid failingSourceId, TechItem item) : ISourceReader
    {
        public SourceType SourceType => SourceType.Rss;

        public Task<IReadOnlyCollection<TechItem>> ReadAsync(
            SourceDefinition source,
            CancellationToken cancellationToken)
        {
            if (source.Id == failingSourceId)
            {
                throw new InvalidOperationException("Feed failed.");
            }

            return Task.FromResult<IReadOnlyCollection<TechItem>>([item]);
        }
    }
}

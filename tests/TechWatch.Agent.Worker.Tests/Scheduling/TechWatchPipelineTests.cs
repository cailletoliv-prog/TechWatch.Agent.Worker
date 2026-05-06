using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker.Models;
using TechWatch.Agent.Worker.Scheduling;
using TechWatch.Agent.Worker.Sources;

namespace TechWatch.Agent.Worker.Tests.Scheduling;

public sealed class TechWatchPipelineTests
{
    [Fact]
    public async Task Run_async_handles_no_items()
    {
        var sourceAggregator = Substitute.For<ISourceAggregator>();
        var contentFilter = Substitute.For<IContentFilter>();
        var logger = new TestLogger<TechWatchPipeline>();
        sourceAggregator
            .FetchAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<TechItem>>([]));
        var pipeline = new TechWatchPipeline(sourceAggregator, contentFilter, logger);

        await pipeline.RunAsync(CancellationToken.None);

        contentFilter.DidNotReceive().Evaluate(Arg.Any<TechItem>());
        logger.Messages.Should().Contain(message => message.Contains("no items fetched"));
    }

    [Fact]
    public async Task Run_async_logs_relevant_item()
    {
        var sourceAggregator = Substitute.For<ISourceAggregator>();
        var contentFilter = Substitute.For<IContentFilter>();
        var logger = new TestLogger<TechWatchPipeline>();
        var item = CreateItem("ASP.NET Core release notes");
        sourceAggregator
            .FetchAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<TechItem>>([item]));
        contentFilter
            .Evaluate(item)
            .Returns(new ContentFilterResult
            {
                IsRelevant = true,
                Score = 2,
                Reason = "Accepted by test."
            });
        var pipeline = new TechWatchPipeline(sourceAggregator, contentFilter, logger);

        await pipeline.RunAsync(CancellationToken.None);

        contentFilter.Received(1).Evaluate(item);
        logger.Messages.Should().Contain(message => message.Contains("relevant item detected"));
        logger.Messages.Should().Contain(message => message.Contains("relevant items 1"));
    }

    [Fact]
    public async Task Run_async_counts_rejected_item()
    {
        var sourceAggregator = Substitute.For<ISourceAggregator>();
        var contentFilter = Substitute.For<IContentFilter>();
        var logger = new TestLogger<TechWatchPipeline>();
        var item = CreateItem("Crypto marketing campaign");
        sourceAggregator
            .FetchAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<TechItem>>([item]));
        contentFilter
            .Evaluate(item)
            .Returns(new ContentFilterResult
            {
                IsRelevant = false,
                Score = 0,
                Reason = "Rejected by test."
            });
        var pipeline = new TechWatchPipeline(sourceAggregator, contentFilter, logger);

        await pipeline.RunAsync(CancellationToken.None);

        contentFilter.Received(1).Evaluate(item);
        logger.Messages.Should().NotContain(message => message.Contains("relevant item detected"));
        logger.Messages.Should().Contain(message => message.Contains("rejected items 1"));
    }

    [Fact]
    public async Task Run_async_propagates_source_aggregator_exceptions()
    {
        var sourceAggregator = Substitute.For<ISourceAggregator>();
        var contentFilter = Substitute.For<IContentFilter>();
        var logger = new TestLogger<TechWatchPipeline>();
        sourceAggregator
            .FetchAsync(Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyCollection<TechItem>>>(_ => throw new InvalidOperationException("Fetch failed."));
        var pipeline = new TechWatchPipeline(sourceAggregator, contentFilter, logger);

        var act = () => pipeline.RunAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Fetch failed.");
        contentFilter.DidNotReceive().Evaluate(Arg.Any<TechItem>());
    }

    private static TechItem CreateItem(string title)
    {
        return new TechItem
        {
            SourceId = Guid.NewGuid(),
            SourceName = "Test Source",
            SourceType = SourceType.Rss,
            Title = title,
            Url = "https://example.com/item",
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z")
        };
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}

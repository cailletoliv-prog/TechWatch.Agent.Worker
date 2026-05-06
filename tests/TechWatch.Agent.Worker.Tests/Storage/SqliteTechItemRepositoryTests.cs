using FluentAssertions;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Models;
using TechWatch.Agent.Worker.Storage;

namespace TechWatch.Agent.Worker.Tests.Storage;

public sealed class SqliteTechItemRepositoryTests : IDisposable
{
    private readonly string databasePath = Path.Combine(
        Path.GetTempPath(),
        "TechWatch.Agent.Worker.Tests",
        $"{Guid.NewGuid():N}.db");

    [Fact]
    public async Task Initialize_async_creates_database_and_allows_insert()
    {
        var repository = CreateRepository();
        var item = CreateItem("https://example.com/dotnet");

        await repository.InitializeAsync(CancellationToken.None);
        var inserted = await repository.UpsertAsync(item, CancellationToken.None);

        inserted.Should().BeTrue();
        File.Exists(databasePath).Should().BeTrue();
        var exists = await repository.ExistsByUrlAsync(item.Url, CancellationToken.None);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Upsert_async_ignores_duplicate_url()
    {
        var repository = CreateRepository();
        var first = CreateItem("https://example.com/duplicate");
        var duplicate = CreateItem("https://example.com/duplicate");

        await repository.InitializeAsync(CancellationToken.None);
        var firstInserted = await repository.UpsertAsync(first, CancellationToken.None);
        var secondInserted = await repository.UpsertAsync(duplicate, CancellationToken.None);

        firstInserted.Should().BeTrue();
        secondInserted.Should().BeFalse();
    }

    [Fact]
    public async Task Get_pending_analysis_async_returns_only_pending_items()
    {
        var repository = CreateRepository();
        var pending = CreateItem("https://example.com/pending", TechItemStatus.PendingAnalysis);
        var analyzed = CreateItem("https://example.com/analyzed", TechItemStatus.Analyzed);

        await repository.InitializeAsync(CancellationToken.None);
        await repository.UpsertAsync(pending, CancellationToken.None);
        await repository.UpsertAsync(analyzed, CancellationToken.None);

        var items = await repository.GetPendingAnalysisAsync(10, CancellationToken.None);

        var item = items.Should().ContainSingle().Subject;
        item.Url.Should().Be(pending.Url);
        item.Status.Should().Be(TechItemStatus.PendingAnalysis);
    }

    public void Dispose()
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }

    private SqliteTechItemRepository CreateRepository()
    {
        var options = Options.Create(new StorageOptions
        {
            ConnectionString = $"Data Source={databasePath};Pooling=False"
        });
        var initializer = new DatabaseInitializer(options);

        return new SqliteTechItemRepository(options, initializer);
    }

    private static TechItem CreateItem(
        string url,
        TechItemStatus status = TechItemStatus.PendingAnalysis)
    {
        return new TechItem
        {
            SourceId = Guid.NewGuid(),
            SourceName = "Test Source",
            SourceType = SourceType.Rss,
            Title = "Test item",
            Url = url,
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z"),
            Summary = "Summary",
            Content = "Content",
            Status = status
        };
    }
}

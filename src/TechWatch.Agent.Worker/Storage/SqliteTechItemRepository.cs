using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Storage;

public sealed class SqliteTechItemRepository(
    IOptions<StorageOptions> options,
    DatabaseInitializer databaseInitializer) : ITechItemRepository
{
    private readonly string connectionString = options.Value.ConnectionString;

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return databaseInitializer.InitializeAsync(cancellationToken);
    }

    public async Task<bool> UpsertAsync(TechItem item, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT OR IGNORE INTO TechItems (
                    Id,
                    SourceName,
                    SourceType,
                    Title,
                    Url,
                    PublishedAt,
                    Summary,
                    Content,
                    Status,
                    CreatedAt,
                    UpdatedAt
                )
                VALUES (
                    @Id,
                    @SourceName,
                    @SourceType,
                    @Title,
                    @Url,
                    @PublishedAt,
                    @Summary,
                    @Content,
                    @Status,
                    @CreatedAt,
                    @UpdatedAt
                );
                """,
                new
                {
                    Id = item.Id.ToString(),
                    item.SourceName,
                    SourceType = (int)item.SourceType,
                    item.Title,
                    item.Url,
                    PublishedAt = item.PublishedAt.ToString("O"),
                    item.Summary,
                    item.Content,
                    Status = (int)item.Status,
                    CreatedAt = now.ToString("O"),
                    UpdatedAt = now.ToString("O")
                },
                cancellationToken: cancellationToken));

        return affectedRows > 0;
    }

    public async Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(1)
                FROM TechItems
                WHERE Url = @Url;
                """,
                new { Url = url },
                cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<IReadOnlyCollection<TechItem>> GetPendingAnalysisAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<TechItemRow>(
            new CommandDefinition(
                """
                SELECT
                    Id,
                    SourceName,
                    SourceType,
                    Title,
                    Url,
                    PublishedAt,
                    Summary,
                    Content,
                    Status,
                    CreatedAt
                FROM TechItems
                WHERE Status = @Status
                ORDER BY PublishedAt DESC
                LIMIT @Limit;
                """,
                new
                {
                    Status = (int)TechItemStatus.PendingAnalysis,
                    Limit = limit
                },
                cancellationToken: cancellationToken));

        return rows.Select(ToTechItem).ToArray();
    }

    private static TechItem ToTechItem(TechItemRow row)
    {
        return new TechItem
        {
            Id = Guid.Parse(row.Id),
            SourceName = row.SourceName,
            SourceType = (SourceType)row.SourceType,
            Title = row.Title,
            Url = row.Url,
            PublishedAt = DateTimeOffset.Parse(row.PublishedAt),
            FetchedAt = DateTimeOffset.Parse(row.CreatedAt),
            Summary = row.Summary,
            Content = row.Content,
            Status = (TechItemStatus)row.Status
        };
    }

    private sealed class TechItemRow
    {
        public string Id { get; init; } = string.Empty;

        public string SourceName { get; init; } = string.Empty;

        public int SourceType { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Url { get; init; } = string.Empty;

        public string PublishedAt { get; init; } = string.Empty;

        public string? Summary { get; init; }

        public string? Content { get; init; }

        public int Status { get; init; }

        public string CreatedAt { get; init; } = string.Empty;
    }
}

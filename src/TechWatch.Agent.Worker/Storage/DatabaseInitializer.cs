using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;

namespace TechWatch.Agent.Worker.Storage;

public sealed class DatabaseInitializer(IOptions<StorageOptions> options)
{
    private readonly string connectionString = options.Value.ConnectionString;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        EnsureDatabaseDirectoryExists();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                CREATE TABLE IF NOT EXISTS TechItems (
                    Id TEXT NOT NULL PRIMARY KEY,
                    SourceName TEXT NOT NULL,
                    SourceType INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Url TEXT NOT NULL,
                    PublishedAt TEXT NOT NULL,
                    Summary TEXT NULL,
                    Content TEXT NULL,
                    Status INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    CONSTRAINT UX_TechItems_Url UNIQUE (Url)
                );
                """,
                cancellationToken: cancellationToken));
    }

    private void EnsureDatabaseDirectoryExists()
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

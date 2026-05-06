using Microsoft.Extensions.Options;
using Microsoft.Data.Sqlite;

namespace TechWatch.Agent.Worker.Configuration;

public sealed class AppPathResolver(IOptions<PathOptions> options)
{
    public string Resolve(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
        {
            return path;
        }

        var baseDirectory = string.IsNullOrWhiteSpace(options.Value.BaseDirectory)
            ? AppContext.BaseDirectory
            : options.Value.BaseDirectory;

        return Path.GetFullPath(Path.Combine(baseDirectory, path));
    }

    public string ResolveSqliteConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(builder.DataSource)
            && !builder.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            builder.DataSource = Resolve(builder.DataSource);
        }

        return builder.ToString();
    }
}

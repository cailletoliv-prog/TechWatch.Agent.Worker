namespace TechWatch.Agent.Worker.Configuration;

public sealed class StorageOptions
{
    public string ConnectionString { get; init; } = "Data Source=data/techwatch.db";
}

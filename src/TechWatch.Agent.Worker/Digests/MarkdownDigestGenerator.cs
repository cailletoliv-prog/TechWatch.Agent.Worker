using System.Text;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Digests;

public sealed class MarkdownDigestGenerator(
    IOptions<DigestOptions> options,
    AppPathResolver pathResolver) : IDigestGenerator
{
    private readonly DigestOptions options = options.Value;

    public async Task<DigestRun> GenerateAsync(
        IReadOnlyCollection<DigestEntry> entries,
        DateTimeOffset runDate,
        CancellationToken cancellationToken)
    {
        var outputDirectory = pathResolver.Resolve(options.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var orderedEntries = entries
            .OrderByDescending(entry => entry.InterestScore)
            .ThenByDescending(entry => entry.PublishedAt)
            .ToArray();

        var outputPath = Path.Combine(
            outputDirectory,
            $"{runDate.ToString(options.FileNameFormat)}.md");

        var markdown = BuildMarkdown(orderedEntries, runDate);
        await File.WriteAllTextAsync(outputPath, markdown, cancellationToken);

        return new DigestRun
        {
            RunDate = runDate,
            OutputPath = outputPath,
            Entries = orderedEntries
        };
    }

    private static string BuildMarkdown(IReadOnlyCollection<DigestEntry> entries, DateTimeOffset runDate)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Tech Watch Digest - {runDate:dd/MM/yyyy}");
        builder.AppendLine();

        var categorizedEntries = Categorize(entries);
        foreach (var category in categorizedEntries)
        {
            AppendSection(builder, category.Title, category.Entries);
        }

        return builder.ToString();
    }

    private static IReadOnlyCollection<DigestCategory> Categorize(IReadOnlyCollection<DigestEntry> entries)
    {
        var orderedEntries = entries
            .OrderByDescending(entry => entry.InterestScore)
            .ThenByDescending(entry => entry.PublishedAt)
            .ToArray();
        var categoryTitles = new[]
        {
            "Sécurité / Breaking changes",
            "IA / Agents",
            ".NET / ASP.NET Core",
            "EF Core / Data",
            "Tooling / Dev productivity",
            "À surveiller",
            "Faible priorité"
        };

        return categoryTitles
            .Select(title => new DigestCategory(
                title,
                orderedEntries.Where(entry => GetCategoryTitle(entry) == title).ToArray()))
            .ToArray();
    }

    private static string GetCategoryTitle(DigestEntry entry)
    {
        if (IsSecurityOrBreakingChange(entry))
        {
            return "Sécurité / Breaking changes";
        }

        if (entry.InterestScore < 5)
        {
            return "Faible priorité";
        }

        if (HasAnyTag(entry, "ai-dev", "ai", "llm", "agent", "agents", "mcp"))
        {
            return "IA / Agents";
        }

        if (HasAnyTag(entry, "dotnet", ".net", "aspnetcore", "asp.net core", "csharp", "c#"))
        {
            return ".NET / ASP.NET Core";
        }

        if (HasAnyTag(entry, "efcore", "ef core", "entity framework", "data", "oracle", "sql"))
        {
            return "EF Core / Data";
        }

        if (HasAnyTag(entry, "tooling", "productivity", "dev productivity", "workflow", "ci/cd", "sdk"))
        {
            return "Tooling / Dev productivity";
        }

        return "À surveiller";
    }

    private static bool IsSecurityOrBreakingChange(DigestEntry entry)
    {
        return entry.HasBreakingChange || HasAnyTag(entry, "security", "breaking-change", "breaking change");
    }

    private static bool HasAnyTag(DigestEntry entry, params string[] expectedTags)
    {
        return entry.Tags.Any(tag => expectedTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    private static void AppendSection(
        StringBuilder builder,
        string title,
        IEnumerable<DigestEntry> entries)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();

        var sectionEntries = entries.ToArray();
        if (sectionEntries.Length == 0)
        {
            builder.AppendLine("_No items._");
            builder.AppendLine();
            return;
        }

        foreach (var entry in sectionEntries)
        {
            builder.AppendLine($"### [{entry.Title}]({entry.Url})");
            builder.AppendLine();
            builder.AppendLine($"- Score: {entry.InterestScore}/10");
            builder.AppendLine($"- Source: {entry.SourceName}");
            builder.AppendLine($"- Published: {entry.PublishedAt:dd/MM/yyyy}");

            if (!string.IsNullOrWhiteSpace(entry.Importance))
            {
                builder.AppendLine($"- Importance: {entry.Importance}");
            }

            if (entry.Tags.Count > 0)
            {
                builder.AppendLine($"- Tags: {string.Join(", ", entry.Tags)}");
            }

            if (!string.IsNullOrWhiteSpace(entry.Summary))
            {
                builder.AppendLine();
                builder.AppendLine("Impact:");
                builder.AppendLine(entry.Summary);
            }

            if (!string.IsNullOrWhiteSpace(entry.Reason))
            {
                builder.AppendLine();
                builder.AppendLine($"A retenir: {entry.Reason}");
            }

            builder.AppendLine();
        }
    }

    private sealed record DigestCategory(
        string Title,
        IReadOnlyCollection<DigestEntry> Entries);
}

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

        AppendSection(builder, "Highlights", entries.Where(entry => entry.InterestScore >= 8 && !entry.HasBreakingChange));
        AppendSection(builder, "Breaking changes", entries.Where(entry => entry.HasBreakingChange));
        AppendSection(builder, "Articles", entries.Where(entry => entry.InterestScore is >= 5 and < 8 && !entry.HasBreakingChange));
        AppendSection(builder, "Low priority", entries.Where(entry => entry.InterestScore < 5 && !entry.HasBreakingChange));

        return builder.ToString();
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
}

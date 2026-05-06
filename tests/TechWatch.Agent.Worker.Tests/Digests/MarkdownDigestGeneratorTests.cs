using FluentAssertions;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Digests;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Tests.Digests;

public sealed class MarkdownDigestGeneratorTests : IDisposable
{
    private readonly string outputDirectory = Path.Combine(
        Path.GetTempPath(),
        "TechWatch.Agent.Worker.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Generate_async_writes_markdown_digest_with_expected_sections()
    {
        var generator = new MarkdownDigestGenerator(
            Options.Create(new DigestOptions
            {
                OutputDirectory = outputDirectory,
                FileNameFormat = "yyyy-MM-dd"
            }),
            CreatePathResolver());
        var runDate = DateTimeOffset.Parse("2026-05-06T10:00:00Z");
        var entries = new[]
        {
            CreateEntry("Low item", 3, breakingChange: false, tags: ["tutorial"]),
            CreateEntry("AI item", 7, breakingChange: false, tags: ["mcp", "ai-dev"]),
            CreateEntry("Dotnet high item", 9, breakingChange: false, tags: ["dotnet"]),
            CreateEntry("Dotnet medium item", 6, breakingChange: false, tags: ["aspnetcore"]),
            CreateEntry("Data item", 7, breakingChange: false, tags: ["efcore"]),
            CreateEntry("Tooling item", 6, breakingChange: false, tags: ["tooling"]),
            CreateEntry("Watch item", 5, breakingChange: false, tags: ["roadmap"]),
            CreateEntry("Breaking item", 7, breakingChange: true, tags: ["dotnet", "breaking-change"])
        };

        var run = await generator.GenerateAsync(entries, runDate, CancellationToken.None);

        Path.GetFileName(run.OutputPath).Should().Be("2026-05-06.md");
        File.Exists(run.OutputPath).Should().BeTrue();
        var markdown = await File.ReadAllTextAsync(run.OutputPath);
        markdown.Should().Contain("# Tech Watch Digest - 06/05/2026");
        markdown.Should().Contain("## Securite / Breaking changes");
        markdown.Should().Contain("## IA / Agents");
        markdown.Should().Contain("## .NET / ASP.NET Core");
        markdown.Should().Contain("## EF Core / Data");
        markdown.Should().Contain("## Tooling / Dev productivity");
        markdown.Should().Contain("## A surveiller");
        markdown.Should().Contain("## Faible priorite");
        markdown.IndexOf("Dotnet high item", StringComparison.Ordinal)
            .Should().BeLessThan(markdown.IndexOf("Dotnet medium item", StringComparison.Ordinal));
        markdown.Should().Contain("- Score: 9/10");
        markdown.Should().Contain("- Published: 06/05/2026");
        markdown.Should().Contain("- Tags: dotnet");
        markdown.Should().Contain("Impact:");
        markdown.Should().Contain("A retenir:");
        markdown.IndexOf("## Securite / Breaking changes", StringComparison.Ordinal)
            .Should().BeLessThan(markdown.IndexOf("Breaking item", StringComparison.Ordinal));
        CountOccurrences(markdown, "### [Breaking item]").Should().Be(1);
    }

    public void Dispose()
    {
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    private static DigestEntry CreateEntry(
        string title,
        int score,
        bool breakingChange,
        IReadOnlyCollection<string> tags)
    {
        return new DigestEntry
        {
            TechItemId = Guid.NewGuid(),
            AnalysisResultId = Guid.NewGuid(),
            Title = title,
            Url = $"https://example.com/{title.Replace(' ', '-')}",
            SourceName = "Test Source",
            Summary = $"{title} summary.",
            InterestScore = score,
            Importance = score >= 8 ? "High" : "Medium",
            HasBreakingChange = breakingChange,
            Tags = tags,
            Reason = $"{title} reason.",
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z")
        };
    }

    private static int CountOccurrences(string value, string pattern)
    {
        var count = 0;
        var index = 0;

        while ((index = value.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private static AppPathResolver CreatePathResolver()
    {
        return new AppPathResolver(Options.Create(new PathOptions()));
    }
}

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
        var generator = new MarkdownDigestGenerator(Options.Create(new DigestOptions
        {
            OutputDirectory = outputDirectory,
            FileNameFormat = "yyyy-MM-dd"
        }));
        var runDate = DateTimeOffset.Parse("2026-05-06T10:00:00Z");
        var entries = new[]
        {
            CreateEntry("Low item", 3, breakingChange: false),
            CreateEntry("Highlight item", 9, breakingChange: false),
            CreateEntry("Breaking item", 7, breakingChange: true),
            CreateEntry("Article item", 6, breakingChange: false)
        };

        var run = await generator.GenerateAsync(entries, runDate, CancellationToken.None);

        Path.GetFileName(run.OutputPath).Should().Be("2026-05-06.md");
        File.Exists(run.OutputPath).Should().BeTrue();
        var markdown = await File.ReadAllTextAsync(run.OutputPath);
        markdown.Should().Contain("# Tech Watch Digest - 2026-05-06");
        markdown.Should().Contain("## Highlights");
        markdown.Should().Contain("## Breaking changes");
        markdown.Should().Contain("## Articles");
        markdown.Should().Contain("## Low priority");
        markdown.IndexOf("Highlight item", StringComparison.Ordinal)
            .Should().BeLessThan(markdown.IndexOf("Article item", StringComparison.Ordinal));
        markdown.Should().Contain("- Score: 9/10");
        markdown.Should().Contain("- Tags: dotnet, aspnetcore");
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
        bool breakingChange)
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
            Tags = ["dotnet", "aspnetcore"],
            Reason = $"{title} reason.",
            PublishedAt = DateTimeOffset.Parse("2026-05-06T08:00:00Z")
        };
    }
}

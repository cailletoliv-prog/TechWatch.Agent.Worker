using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TechWatch.Agent.Worker.Models;
using TechWatch.Agent.Worker.Sources;

namespace TechWatch.Agent.Worker.Tests.Sources;

public sealed class RssSourceReaderTests
{
    [Fact]
    public async Task Reads_rss_feed_and_normalizes_items()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8" ?>
            <rss version="2.0">
              <channel>
                <title>.NET Blog</title>
                <item>
                  <title>.NET 8 servicing update</title>
                  <link>https://example.com/dotnet-8-servicing</link>
                  <description>Important runtime fixes.</description>
                  <author>dotnet@example.com</author>
                  <pubDate>Wed, 06 May 2026 08:00:00 GMT</pubDate>
                </item>
              </channel>
            </rss>
            """;

        var reader = CreateReader(xml);
        var source = CreateSource("Microsoft .NET Blog", "https://example.com/feed.xml");

        var items = await reader.ReadAsync(source, CancellationToken.None);

        var item = items.Should().ContainSingle().Subject;
        item.SourceId.Should().Be(source.Id);
        item.SourceName.Should().Be("Microsoft .NET Blog");
        item.SourceType.Should().Be(SourceType.Rss);
        item.Title.Should().Be(".NET 8 servicing update");
        item.Url.Should().Be("https://example.com/dotnet-8-servicing");
        item.Summary.Should().Be("Important runtime fixes.");
        item.PublishedAt.Should().Be(DateTimeOffset.Parse("2026-05-06T08:00:00Z"));
        item.Status.Should().Be(TechItemStatus.New);
    }

    [Fact]
    public async Task Reads_atom_feed_and_normalizes_items()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8" ?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>EF Core Releases</title>
              <entry>
                <title>EF Core 9.0.5</title>
                <link href="https://github.com/dotnet/efcore/releases/tag/v9.0.5" />
                <summary>Bug fixes for EF Core.</summary>
                <content type="html">Release notes content.</content>
                <updated>2026-05-06T09:15:00Z</updated>
                <author>
                  <name>dotnet</name>
                </author>
              </entry>
            </feed>
            """;

        var reader = CreateReader(xml);
        var source = CreateSource("EF Core Releases", "https://github.com/dotnet/efcore/releases.atom");

        var items = await reader.ReadAsync(source, CancellationToken.None);

        var item = items.Should().ContainSingle().Subject;
        item.SourceName.Should().Be("EF Core Releases");
        item.Title.Should().Be("EF Core 9.0.5");
        item.Url.Should().Be("https://github.com/dotnet/efcore/releases/tag/v9.0.5");
        item.Author.Should().Be("dotnet");
        item.Summary.Should().Be("Bug fixes for EF Core.");
        item.Content.Should().Be("Release notes content.");
        item.PublishedAt.Should().Be(DateTimeOffset.Parse("2026-05-06T09:15:00Z"));
    }

    private static RssSourceReader CreateReader(string xml)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(xml));
        var httpClientFactory = new StubHttpClientFactory(httpClient);

        return new RssSourceReader(httpClientFactory, NullLogger<RssSourceReader>.Instance);
    }

    private static SourceDefinition CreateSource(string name, string url)
    {
        return new SourceDefinition
        {
            Name = name,
            Type = SourceType.Rss,
            Url = url
        };
    }

    private sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return httpClient;
        }
    }

    private sealed class StubHttpMessageHandler(string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };

            return Task.FromResult(response);
        }
    }
}

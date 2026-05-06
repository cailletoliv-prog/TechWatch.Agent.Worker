using System.ServiceModel.Syndication;
using System.Xml;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Sources;

public sealed class RssSourceReader(
    IHttpClientFactory httpClientFactory,
    ILogger<RssSourceReader> logger) : ISourceReader
{
    public SourceType SourceType => SourceType.Rss;

    public async Task<IReadOnlyCollection<TechItem>> ReadAsync(
        SourceDefinition source,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient(nameof(RssSourceReader));
        using var response = await client.GetAsync(source.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true
        });

        var feed = SyndicationFeed.Load(reader);
        if (feed is null)
        {
            logger.LogWarning("RSS source {SourceName} returned an empty feed", source.Name);
            return [];
        }

        return feed.Items
            .Select(item => ToTechItem(source, item))
            .Where(item => !string.IsNullOrWhiteSpace(item.Title) && !string.IsNullOrWhiteSpace(item.Url))
            .ToArray();
    }

    private static TechItem ToTechItem(SourceDefinition source, SyndicationItem item)
    {
        return new TechItem
        {
            SourceId = source.Id,
            SourceName = source.Name,
            SourceType = source.Type,
            Title = item.Title?.Text ?? string.Empty,
            Url = GetUrl(item),
            Author = GetAuthor(item),
            Summary = item.Summary?.Text,
            Content = GetContent(item),
            PublishedAt = GetPublishedAt(item),
            Status = TechItemStatus.New
        };
    }

    private static string GetUrl(SyndicationItem item)
    {
        return item.Links.FirstOrDefault(link => link.Uri is not null)?.Uri.ToString()
            ?? item.Id
            ?? string.Empty;
    }

    private static string? GetAuthor(SyndicationItem item)
    {
        return item.Authors.FirstOrDefault()?.Name;
    }

    private static string? GetContent(SyndicationItem item)
    {
        return item.Content switch
        {
            TextSyndicationContent textContent => textContent.Text,
            _ => null
        };
    }

    private static DateTimeOffset GetPublishedAt(SyndicationItem item)
    {
        if (item.PublishDate != DateTimeOffset.MinValue)
        {
            return item.PublishDate;
        }

        if (item.LastUpdatedTime != DateTimeOffset.MinValue)
        {
            return item.LastUpdatedTime;
        }

        return DateTimeOffset.UtcNow;
    }
}

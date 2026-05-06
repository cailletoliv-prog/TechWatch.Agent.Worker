using System.Text.Json;
using System.Text.Json.Serialization;
using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Llm;

public sealed class OllamaContentAnalyzer(IOllamaClient ollamaClient) : IContentAnalyzer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AnalysisResult> AnalyzeAsync(
        TechItem item,
        CancellationToken cancellationToken)
    {
        var response = await ollamaClient.GenerateAsync(BuildPrompt(item), cancellationToken);

        return TryParse(item.Id, response)
            ?? CreateFallback(item.Id);
    }

    public static AnalysisResult? TryParse(Guid techItemId, string response)
    {
        var json = ExtractJsonObject(response);
        if (json is null)
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<OllamaAnalysisPayload>(json, JsonOptions);
            if (payload is null)
            {
                return null;
            }

            return new AnalysisResult
            {
                TechItemId = techItemId,
                InterestScore = Math.Clamp(payload.InterestScore, 0, 10),
                Summary = payload.Summary ?? string.Empty,
                Importance = payload.Importance ?? "Unknown",
                HasBreakingChange = payload.IsBreakingChange,
                Tags = payload.Tags ?? [],
                Reason = payload.Reason ?? string.Empty
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildPrompt(TechItem item)
    {
        return $$"""
            You are a technical watch assistant focused on C#, .NET, ASP.NET Core, EF Core, Oracle, Angular, and AI-assisted development.

            Analyze this technical item and answer with strict JSON only. Do not include markdown or explanations outside JSON.

            Expected JSON shape:
            {
              "interestScore": 0,
              "summary": "short summary",
              "importance": "Low|Medium|High",
              "isBreakingChange": false,
              "tags": ["tag"],
              "reason": "short reason"
            }

            Item:
            Title: {{item.Title}}
            Source: {{item.SourceName}}
            Url: {{item.Url}}
            PublishedAt: {{item.PublishedAt:O}}
            Summary: {{item.Summary ?? string.Empty}}
            Content: {{item.Content ?? string.Empty}}
            """;
    }

    private static AnalysisResult CreateFallback(Guid techItemId)
    {
        return new AnalysisResult
        {
            TechItemId = techItemId,
            InterestScore = 0,
            Summary = "Analysis unavailable.",
            Importance = "Unknown",
            HasBreakingChange = false,
            Tags = [],
            Reason = "Ollama response could not be parsed."
        };
    }

    private static string? ExtractJsonObject(string value)
    {
        var start = value.IndexOf('{');
        var end = value.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return value[start..(end + 1)];
    }

    private sealed class OllamaAnalysisPayload
    {
        public int InterestScore { get; init; }

        public string? Summary { get; init; }

        public string? Importance { get; init; }

        [JsonPropertyName("isBreakingChange")]
        public bool IsBreakingChange { get; init; }

        public IReadOnlyCollection<string>? Tags { get; init; }

        public string? Reason { get; init; }
    }
}

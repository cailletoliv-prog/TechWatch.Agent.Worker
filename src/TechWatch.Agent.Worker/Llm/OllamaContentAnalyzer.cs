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
            You are a senior technical watch assistant for a lead developer working mainly with:
            C#, .NET, ASP.NET Core, EF Core, Oracle, Angular, TypeScript, and AI-assisted software development.

            Analyze this item for that profile. Answer with strict JSON only. Do not include markdown or explanations outside JSON.

            Keep the original title and source language untouched in your reasoning, but write summary, importance, reason, and any impact wording in natural, professional French.
            Use a sober, factual, senior developer tone. Avoid promotional wording and overconfident conclusions.
            Do not claim that the item directly impacts our projects unless the text explicitly proves it.
            Prefer formulations such as "peut etre pertinent", "a surveiller", "a verifier dans nos contextes", or "faible priorite" when the impact is indirect.
            Avoid "nous devons", "il est essentiel", and "crucial" unless the item is a real security issue or a confirmed major breaking change.
            Avoid literal or awkward translations. Do not translate standard technical terms when they are commonly used in French technical teams.
            Keep terms such as MCP, tooling, breaking change, workflow, security, endpoint, runtime, SDK, release notes, migration, API, preview, LTS, CI/CD, and performance in English when appropriate.
            Never invent odd French words for established English technical terms.
            Preferred French wording examples: "multi-tours", "nettoyer la reponse", "filtrer la reponse", "controle des appels MCP".

            Scoring guidance:
            - 9-10: critical security, major confirmed breaking change, or strong direct impact for the profile.
            - 7-8: important topic to watch or read for architecture, framework usage, migration, performance, tooling, or roadmap.
            - 5-6: interesting but optional.
            - 3-4: low priority.
            - 0-2: noise, marketing, duplicate, vague, inspirational, or not directly useful for the profile.
            Do not over-score inspirational articles, demos, tutorials, or product showcases unless they contain concrete technical details, migration guidance, security information, or actionable engineering impact.

            Tags rules:
            - Use only topics explicitly present in the item.
            - Prefer concise tags such as dotnet, aspnetcore, efcore, oracle, angular, typescript, ai-dev, tooling, performance, security.
            - Do not invent tags.

            Field guidance:
            - summary: 2-4 short factual sentences in natural professional French. Mention concrete impact only when supported by the item.
            - reason: justify the score in one factual sentence in natural professional French. Do not make unproven claims about our projects.
            - isBreakingChange: true only if the item mentions or strongly implies migration risk, removed APIs, changed defaults, incompatible behavior, or required action.

            Expected JSON shape:
            {
              "interestScore": 0,
              "summary": "resume actionnable en francais naturel et professionnel",
              "importance": "Low|Medium|High",
              "isBreakingChange": false,
              "tags": ["tag"],
              "reason": "raison en francais naturel et professionnel"
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
            Summary = "Analyse indisponible.",
            Importance = "Unknown",
            HasBreakingChange = false,
            Tags = [],
            Reason = "La reponse Ollama n'a pas pu etre analysee."
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

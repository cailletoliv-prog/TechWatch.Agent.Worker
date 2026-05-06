using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TechWatch.Agent.Worker.Configuration;

namespace TechWatch.Agent.Worker.Llm;

public sealed class OllamaClient(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options) : IOllamaClient
{
    private readonly OllamaOptions options = options.Value;

    public async Task<string> GenerateAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

        using var client = httpClientFactory.CreateClient(nameof(OllamaClient));
        client.BaseAddress = new Uri(options.BaseUrl);

        var request = new OllamaGenerateRequest(
            options.Model,
            prompt,
            Stream: false);

        using var response = await client.PostAsJsonAsync("/api/generate", request, timeout.Token);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
            cancellationToken: timeout.Token);

        return payload?.Response ?? string.Empty;
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string Response);
}

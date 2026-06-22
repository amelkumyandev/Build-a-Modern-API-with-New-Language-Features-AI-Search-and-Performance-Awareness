using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LocalKnowledgeIntelligence.Application;
using Microsoft.Extensions.Options;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class OpenAiEmbeddingClient(HttpClient httpClient, IOptions<OpenAiOptions> options) : IEmbeddingClient
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _options.EmbeddingModel,
            input,
            dimensions = _options.EmbeddingDimensions
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ConfigurationException($"OpenAI embedding request failed with status {(int)response.StatusCode}: {detail}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return json.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ConfigurationException("OpenAI API key is not configured. Set OPENAI_API_KEY in environment variables or user secrets.");
        }
    }
}

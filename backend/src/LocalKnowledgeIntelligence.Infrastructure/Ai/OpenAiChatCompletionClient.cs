using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LocalKnowledgeIntelligence.Application;
using Microsoft.Extensions.Options;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class OpenAiChatCompletionClient(HttpClient httpClient, IOptions<OpenAiOptions> options) : IChatCompletionClient
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<GeneratedAnswer> GenerateAnswerAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ConfigurationException("OpenAI API key is not configured. Set OPENAI_API_KEY in environment variables or user secrets.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = _options.ChatModel,
            input = prompt
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ConfigurationException($"OpenAI answer generation failed with status {(int)response.StatusCode}: {detail}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var answer = ExtractResponseText(json.RootElement);
        return new GeneratedAnswer(string.IsNullOrWhiteSpace(answer) ? "The model returned an empty answer." : answer);
    }

    private static string ExtractResponseText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var output))
        {
            return string.Empty;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content))
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }
}

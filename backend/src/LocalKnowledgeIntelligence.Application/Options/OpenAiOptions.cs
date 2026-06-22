namespace LocalKnowledgeIntelligence.Application;

public sealed class OpenAiOptions
{
    public string ApiKey
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    public string EmbeddingModel
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? "text-embedding-3-small" : value.Trim();
    } = "text-embedding-3-small";

    public string ChatModel
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? "gpt-4.1-mini" : value.Trim();
    } = "gpt-4.1-mini";

    public int EmbeddingDimensions
    {
        get;
        set => field = Math.Max(1, value);
    } = 1536;
}

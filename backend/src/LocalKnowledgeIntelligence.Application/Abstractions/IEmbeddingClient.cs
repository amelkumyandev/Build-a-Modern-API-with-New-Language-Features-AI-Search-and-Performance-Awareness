namespace LocalKnowledgeIntelligence.Application;

public interface IEmbeddingClient
{
    Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken);
}

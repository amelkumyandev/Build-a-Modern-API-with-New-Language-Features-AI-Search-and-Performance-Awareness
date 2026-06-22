namespace LocalKnowledgeIntelligence.Application;

public interface ISearchRepository
{
    Task<IReadOnlyList<SearchHit>> KeywordSearchAsync(string query, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<SearchHit>> SemanticSearchAsync(float[] embedding, int limit, CancellationToken cancellationToken);
    Task StoreEmbeddingAsync(Guid chunkId, float[] embedding, string model, int dimensions, DateTimeOffset generatedAt, CancellationToken cancellationToken);
}

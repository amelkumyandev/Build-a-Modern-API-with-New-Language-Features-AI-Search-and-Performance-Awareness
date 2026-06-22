namespace LocalKnowledgeIntelligence.Contracts;

public sealed record ChunkResponse(
    Guid Id,
    Guid DocumentId,
    int ChunkIndex,
    string Content,
    int TokenEstimate,
    Dictionary<string, object?> Metadata,
    string? EmbeddingModel,
    int? EmbeddingDimensions,
    DateTimeOffset? EmbeddingGeneratedAt);

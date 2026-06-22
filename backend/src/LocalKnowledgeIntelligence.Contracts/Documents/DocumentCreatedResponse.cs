namespace LocalKnowledgeIntelligence.Contracts;

public sealed record DocumentCreatedResponse(
    Guid Id,
    string Title,
    string Status,
    string ChunkingStatus,
    string EmbeddingStatus);

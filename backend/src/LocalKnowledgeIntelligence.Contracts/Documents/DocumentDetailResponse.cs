namespace LocalKnowledgeIntelligence.Contracts;

public sealed record DocumentDetailResponse(
    Guid Id,
    string Title,
    string Content,
    string? Summary,
    IReadOnlyList<string> Tags,
    Dictionary<string, object?> Metadata,
    string Status,
    string ChunkingStatus,
    string EmbeddingStatus,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

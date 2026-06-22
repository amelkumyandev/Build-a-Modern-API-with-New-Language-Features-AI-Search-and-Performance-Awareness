namespace LocalKnowledgeIntelligence.Contracts;

public sealed record DocumentSummaryResponse(
    Guid Id,
    string Title,
    string? Summary,
    IReadOnlyList<string> Tags,
    string Status,
    string ChunkingStatus,
    string EmbeddingStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

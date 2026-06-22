namespace LocalKnowledgeIntelligence.Contracts;

public sealed record CitationResponse(
    Guid DocumentId,
    Guid ChunkId,
    string Title,
    string Snippet,
    double Score);

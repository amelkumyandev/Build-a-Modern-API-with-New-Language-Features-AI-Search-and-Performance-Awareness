namespace LocalKnowledgeIntelligence.Domain;

public sealed record Citation(Guid DocumentId, Guid ChunkId, string Title, string Snippet, double Score);

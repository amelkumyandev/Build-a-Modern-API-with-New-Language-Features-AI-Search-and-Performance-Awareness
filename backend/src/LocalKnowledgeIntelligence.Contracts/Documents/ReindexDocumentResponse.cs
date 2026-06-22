namespace LocalKnowledgeIntelligence.Contracts;

public sealed record ReindexDocumentResponse(Guid DocumentId, int ChunksCreated, int EmbeddingsGenerated, string Status);

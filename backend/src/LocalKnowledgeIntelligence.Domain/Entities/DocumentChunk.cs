namespace LocalKnowledgeIntelligence.Domain;

public sealed class DocumentChunk
{
    private DocumentChunk()
    {
    }

    private DocumentChunk(Guid id, Guid documentId, int chunkIndex, string content, int tokenEstimate, Dictionary<string, object?> metadata, DateTimeOffset now)
    {
        Id = id;
        DocumentId = documentId;
        ChunkIndex = chunkIndex;
        Content = content.Trim();
        TokenEstimate = tokenEstimate;
        Metadata = new(metadata);
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int ChunkIndex { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int TokenEstimate { get; private set; }
    public Dictionary<string, object?> Metadata { get; private set; } = [];
    public string? EmbeddingModel { get; private set; }
    public int? EmbeddingDimensions { get; private set; }
    public DateTimeOffset? EmbeddingGeneratedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public KnowledgeDocument? Document { get; private set; }

    public static DocumentChunk Create(Guid documentId, int chunkIndex, string content, int tokenEstimate, Dictionary<string, object?> metadata, DateTimeOffset now)
    {
        return new DocumentChunk(Guid.NewGuid(), documentId, chunkIndex, content, tokenEstimate, metadata, now);
    }

    public void MarkEmbedded(string model, int dimensions, DateTimeOffset now)
    {
        EmbeddingModel = model;
        EmbeddingDimensions = dimensions;
        EmbeddingGeneratedAt = now;
        UpdatedAt = now;
    }
}

namespace LocalKnowledgeIntelligence.Domain;

public sealed class KnowledgeDocument
{
    private KnowledgeDocument()
    {
    }

    private KnowledgeDocument(Guid id, string title, string content, IReadOnlyList<string> tags, Dictionary<string, object?> metadata, Guid createdByUserId, DateTimeOffset now)
    {
        Id = id;
        Title = title.Trim();
        SourceType = "manual";
        Content = content.Trim();
        Tags = [.. tags];
        Metadata = new(metadata);
        Status = DocumentStatus.Created;
        ChunkingStatus = ChunkingStatus.Pending;
        EmbeddingStatus = EmbeddingStatus.Pending;
        CreatedByUserId = createdByUserId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string SourceType { get; private set; } = "manual";
    public string Content { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public Dictionary<string, object?> Metadata { get; private set; } = [];
    public DocumentStatus Status { get; private set; }
    public ChunkingStatus ChunkingStatus { get; private set; }
    public EmbeddingStatus EmbeddingStatus { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt is not null;

    public static KnowledgeDocument Create(string title, string content, IEnumerable<string>? tags, Dictionary<string, object?>? metadata, Guid createdByUserId, DateTimeOffset now)
    {
        return new KnowledgeDocument(Guid.NewGuid(), title, content, tags.NormalizeTags(), metadata.NormalizeMetadata(), createdByUserId, now);
    }

    public void Update(string title, string content, IEnumerable<string>? tags, Dictionary<string, object?>? metadata, DocumentStatus status, DateTimeOffset now)
    {
        Title = title.Trim();
        Content = content.Trim();
        Tags = [.. tags.NormalizeTags()];
        Metadata = metadata.NormalizeMetadata();
        Status = status;
        ChunkingStatus = ChunkingStatus.Pending;
        EmbeddingStatus = EmbeddingStatus.Pending;
        UpdatedAt = now;
    }

    public void MarkChunked(DateTimeOffset now)
    {
        ChunkingStatus = ChunkingStatus.Chunked;
        EmbeddingStatus = EmbeddingStatus.Pending;
        UpdatedAt = now;
    }

    public void MarkIndexed(DateTimeOffset now)
    {
        Status = DocumentStatus.Indexed;
        ChunkingStatus = ChunkingStatus.Chunked;
        EmbeddingStatus = EmbeddingStatus.Embedded;
        UpdatedAt = now;
    }

    public void MarkEmbeddingFailed(DateTimeOffset now)
    {
        EmbeddingStatus = EmbeddingStatus.Failed;
        UpdatedAt = now;
    }

    public void MarkDeleted(DateTimeOffset now)
    {
        if (DeletedAt is not null)
        {
            return;
        }

        DeletedAt = now;
        UpdatedAt = now;
    }

    public void SetSummary(string summary, DateTimeOffset now)
    {
        Summary = summary;
        UpdatedAt = now;
    }
}

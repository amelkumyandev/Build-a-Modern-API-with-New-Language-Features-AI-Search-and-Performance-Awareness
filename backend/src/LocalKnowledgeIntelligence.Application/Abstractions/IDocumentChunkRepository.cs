using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public interface IDocumentChunkRepository
{
    Task ReplaceForDocumentAsync(Guid documentId, IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentChunk>> GetChunksMissingEmbeddingsAsync(Guid documentId, CancellationToken cancellationToken);
    Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task<int> CountEmbeddedAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

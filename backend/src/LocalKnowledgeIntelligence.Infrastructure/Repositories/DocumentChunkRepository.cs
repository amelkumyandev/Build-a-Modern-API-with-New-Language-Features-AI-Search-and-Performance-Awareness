using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class DocumentChunkRepository(AppDbContext db) : IDocumentChunkRepository
{
    public async Task ReplaceForDocumentAsync(Guid documentId, IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken)
    {
        var existing = await db.DocumentChunks.Where(chunk => chunk.DocumentId == documentId).ToArrayAsync(cancellationToken);
        db.DocumentChunks.RemoveRange(existing);
        db.DocumentChunks.AddRange(chunks);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await db.DocumentChunks
            .AsNoTracking()
            .Where(chunk => chunk.DocumentId == documentId)
            .OrderBy(chunk => chunk.ChunkIndex)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetChunksMissingEmbeddingsAsync(Guid documentId, CancellationToken cancellationToken)
    {
        return await db.DocumentChunks
            .Where(chunk => chunk.DocumentId == documentId && chunk.EmbeddingGeneratedAt == null)
            .OrderBy(chunk => chunk.ChunkIndex)
            .ToArrayAsync(cancellationToken);
    }

    public Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.DocumentChunks.FirstOrDefaultAsync(chunk => chunk.Id == id, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return db.DocumentChunks.CountAsync(cancellationToken);
    }

    public Task<int> CountEmbeddedAsync(CancellationToken cancellationToken)
    {
        return db.DocumentChunks.CountAsync(chunk => chunk.EmbeddingGeneratedAt != null, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

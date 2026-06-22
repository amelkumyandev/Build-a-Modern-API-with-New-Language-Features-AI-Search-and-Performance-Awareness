using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class DocumentRepository(AppDbContext db) : IDocumentRepository
{
    public async Task AddAsync(KnowledgeDocument document, CancellationToken cancellationToken)
    {
        db.Documents.Add(document);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<KnowledgeDocument?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = db.Documents.AsQueryable();
        if (!includeDeleted)
        {
            query = query.Where(document => document.DeletedAt == null);
        }

        return query.FirstOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public async Task<PagedResult<KnowledgeDocument>> ListAsync(int pageNumber, int pageSize, IReadOnlyList<string> tags, CancellationToken cancellationToken)
    {
        var active = await db.Documents
            .AsNoTracking()
            .Where(document => document.DeletedAt == null)
            .OrderByDescending(document => document.UpdatedAt)
            .ToArrayAsync(cancellationToken);

        if (tags.Count > 0)
        {
            active = active
                .Where(document => document.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .ToArray();
        }

        var total = active.Length;
        var items = active
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return new PagedResult<KnowledgeDocument>(items, pageNumber, pageSize, total);
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> GetAllActiveAsync(CancellationToken cancellationToken)
    {
        return await db.Documents.Where(document => document.DeletedAt == null).ToArrayAsync(cancellationToken);
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken)
    {
        return db.Documents.CountAsync(document => document.DeletedAt == null, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

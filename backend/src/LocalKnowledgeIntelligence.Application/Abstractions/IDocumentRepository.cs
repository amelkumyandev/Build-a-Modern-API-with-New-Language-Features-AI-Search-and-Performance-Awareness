using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public interface IDocumentRepository
{
    Task AddAsync(KnowledgeDocument document, CancellationToken cancellationToken);
    Task<KnowledgeDocument?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);
    Task<PagedResult<KnowledgeDocument>> ListAsync(int pageNumber, int pageSize, IReadOnlyList<string> tags, CancellationToken cancellationToken);
    Task<IReadOnlyList<KnowledgeDocument>> GetAllActiveAsync(CancellationToken cancellationToken);
    Task<int> CountActiveAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

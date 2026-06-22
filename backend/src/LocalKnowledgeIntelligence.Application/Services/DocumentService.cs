using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class DocumentService(
    IDocumentRepository documents,
    IDocumentChunkRepository chunks,
    ISearchRepository search,
    IEmbeddingClient embeddings,
    DocumentValidator validator,
    DocumentChunkingService chunker,
    OpenAiOptions openAiOptions,
    SearchOptions searchOptions,
    IClock clock)
{
    public async Task<DocumentCreatedResponse> CreateAsync(CreateDocumentRequest request, Guid userId, CancellationToken cancellationToken)
    {
        validator.ValidateCreate(request);

        var document = KnowledgeDocument.Create(request.Title, request.Content, request.Tags, request.Metadata, userId, clock.UtcNow);
        await documents.AddAsync(document, cancellationToken);

        return new DocumentCreatedResponse(document.Id, document.Title, document.Status.ToString(), document.ChunkingStatus.ToString(), document.EmbeddingStatus.ToString());
    }

    public async Task<DocumentCreatedResponse> CreateAndIndexAsync(CreateDocumentRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var created = await CreateAsync(request, userId, cancellationToken);
        await ReindexAsync(created.Id, cancellationToken);

        var document = await documents.GetByIdAsync(created.Id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found after indexing.");

        return new DocumentCreatedResponse(document.Id, document.Title, document.Status.ToString(), document.ChunkingStatus.ToString(), document.EmbeddingStatus.ToString());
    }

    public async Task<PagedResponse<DocumentSummaryResponse>> ListAsync(int? pageNumber, int? pageSize, string? tags, CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedSize) = Pagination.Normalize(pageNumber, pageSize, searchOptions.DefaultLimit, searchOptions.MaxLimit);
        var normalizedTags = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).NormalizeTags() ?? [];
        var page = await documents.ListAsync(normalizedPage, normalizedSize, normalizedTags, cancellationToken);
        return new PagedResult<DocumentSummaryResponse>(
            page.Items.Select(item => item.ToSummaryResponse()).ToArray(),
            page.PageNumber,
            page.PageSize,
            page.TotalCount).ToResponse();
    }

    public async Task<DocumentDetailResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        return document.ToDetailResponse();
    }

    public async Task<DocumentDetailResponse> UpdateAsync(Guid id, UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
        var status = validator.ValidateUpdate(request);
        var document = await documents.GetByIdAsync(id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        document.Update(request.Title, request.Content, request.Tags, request.Metadata, status, clock.UtcNow);
        await documents.SaveChangesAsync(cancellationToken);
        return document.ToDetailResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        document.MarkDeleted(clock.UtcNow);
        await documents.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReindexDocumentResponse> ChunkAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        var drafts = chunker.Chunk(document);
        var now = clock.UtcNow;
        var newChunks = drafts
            .Select(draft => DocumentChunk.Create(document.Id, draft.Index, draft.Content, draft.TokenEstimate, draft.Metadata, now))
            .ToArray();

        await chunks.ReplaceForDocumentAsync(document.Id, newChunks, cancellationToken);
        document.MarkChunked(now);
        await documents.SaveChangesAsync(cancellationToken);

        return new ReindexDocumentResponse(document.Id, newChunks.Length, 0, document.Status.ToString());
    }

    public async Task<int> EmbedAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        var missingChunks = await chunks.GetChunksMissingEmbeddingsAsync(document.Id, cancellationToken);
        var generated = 0;

        foreach (var chunk in missingChunks)
        {
            var input = BuildEmbeddingInput(document, chunk);
            var vector = await embeddings.GenerateEmbeddingAsync(input, cancellationToken);
            await search.StoreEmbeddingAsync(chunk.Id, vector, openAiOptions.EmbeddingModel, openAiOptions.EmbeddingDimensions, clock.UtcNow, cancellationToken);
            chunk.MarkEmbedded(openAiOptions.EmbeddingModel, openAiOptions.EmbeddingDimensions, clock.UtcNow);
            generated++;
        }

        if (generated > 0 || missingChunks.Count > 0)
        {
            document.MarkIndexed(clock.UtcNow);
        }

        await chunks.SaveChangesAsync(cancellationToken);
        await documents.SaveChangesAsync(cancellationToken);
        return generated;
    }

    public async Task<ReindexDocumentResponse> ReindexAsync(Guid id, CancellationToken cancellationToken)
    {
        var chunkResponse = await ChunkAsync(id, cancellationToken);
        var embedded = await EmbedAsync(id, cancellationToken);
        return chunkResponse with { EmbeddingsGenerated = embedded, Status = "Indexed" };
    }

    public async Task<IReadOnlyList<ChunkResponse>> GetChunksAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = await documents.GetByIdAsync(id, includeDeleted: false, cancellationToken)
            ?? throw new NotFoundException("Document was not found.");

        var documentChunks = await chunks.GetByDocumentIdAsync(id, cancellationToken);
        return documentChunks.Select(chunk => chunk.ToResponse()).ToArray();
    }

    private static string BuildEmbeddingInput(KnowledgeDocument document, DocumentChunk chunk)
    {
        return $"""
        Document Title: {document.Title}
        Document Tags: {string.Join(", ", document.Tags)}
        Chunk Index: {chunk.ChunkIndex}
        Content:
        {chunk.Content}
        """;
    }
}

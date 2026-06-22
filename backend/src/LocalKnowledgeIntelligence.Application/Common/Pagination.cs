using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Application;

public static class Pagination
{
    public static (int PageNumber, int PageSize) Normalize(int? pageNumber, int? pageSize, int defaultPageSize, int maxPageSize)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber ?? 1);
        var requestedPageSize = pageSize is null or <= 0 ? defaultPageSize : pageSize.Value;
        var normalizedPageSize = Math.Clamp(requestedPageSize, 1, maxPageSize);
        return (normalizedPageNumber, normalizedPageSize);
    }

    public static PagedResponse<T> ToResponse<T>(this PagedResult<T> result)
    {
        return new(result.Items, result.PageNumber, result.PageSize, result.TotalCount, result.TotalPages, result.HasNextPage, result.HasPreviousPage);
    }
}

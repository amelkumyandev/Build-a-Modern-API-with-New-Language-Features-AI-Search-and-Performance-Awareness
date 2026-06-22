using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Api;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Search).RequireAuthorization().WithTags("Search");

        group.MapGet("/keyword", async (string query, int? limit, SearchService search, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await search.KeywordAsync(query, limit, cancellationToken));
        }).WithName("KeywordSearch");

        group.MapGet("/semantic", async (string query, int? limit, SearchService search, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await search.SemanticAsync(query, limit, cancellationToken));
        }).WithName("SemanticSearch");

        group.MapGet("/hybrid", async (string query, int? limit, SearchService search, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await search.HybridAsync(query, limit, cancellationToken));
        }).WithName("HybridSearch");
    }
}

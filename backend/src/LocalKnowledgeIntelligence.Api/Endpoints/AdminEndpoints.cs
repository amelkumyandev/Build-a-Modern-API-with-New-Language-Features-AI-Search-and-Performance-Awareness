using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Api;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder endpoints, OpenAiOptions openAiOptions)
    {
        var group = endpoints.MapGroup(ApiRoutes.Admin).RequireAuthorization().WithTags("Admin");

        group.MapGet("/dashboard", async (AppDbContext db, SeedDataService seed, CancellationToken cancellationToken) =>
        {
            var databaseReady = await db.Database.CanConnectAsync(cancellationToken);
            var response = await seed.GetDashboardAsync(databaseReady, !string.IsNullOrWhiteSpace(openAiOptions.ApiKey), cancellationToken);
            return Results.Ok(response);
        }).WithName("GetAdminDashboard");

        group.MapPost("/seed", async (SeedDataService seed, CancellationToken cancellationToken) =>
        {
            var response = await seed.SeedDemoDataAsync(cancellationToken);
            return Results.Ok(response);
        }).WithName("SeedDemoData");

        group.MapPost("/reindex", async (IDocumentRepository documents, DocumentService service, CancellationToken cancellationToken) =>
        {
            var all = await documents.GetAllActiveAsync(cancellationToken);
            var results = new List<ReindexDocumentResponse>();
            foreach (var document in all)
            {
                results.Add(await service.ReindexAsync(document.Id, cancellationToken));
            }

            return Results.Ok(results);
        }).WithName("ReindexAllDocuments");

        group.MapGet("/settings", (RuntimeSettings settings) => Results.Ok(settings.ToResponse())).WithName("GetSettings");

        group.MapPut("/settings", (UpdateSettingsRequest request, RuntimeSettings settings) =>
        {
            return Results.Ok(settings.Update(request));
        }).WithName("UpdateSettings");
    }
}

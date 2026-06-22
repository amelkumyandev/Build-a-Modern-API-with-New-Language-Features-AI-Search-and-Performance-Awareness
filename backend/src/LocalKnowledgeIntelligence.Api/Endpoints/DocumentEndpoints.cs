using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Api;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Documents).RequireAuthorization().WithTags("Documents");

        group.MapPost("/upload", async (HttpContext context, bool? index, DocumentService service, CancellationToken cancellationToken) =>
        {
            if (!context.Request.HasFormContentType)
            {
                throw ValidationFailureException.For("file", "Upload must use multipart/form-data.");
            }

            var form = await context.Request.ReadFormAsync(cancellationToken);
            var upload = await DocumentUploadFormReader.ReadAsync(form, cancellationToken);
            var shouldIndex = index == true || upload.Index;
            var response = shouldIndex
                ? await service.CreateAndIndexAsync(upload.Request, context.User.GetUserId(), cancellationToken)
                : await service.CreateAsync(upload.Request, context.User.GetUserId(), cancellationToken);

            return Results.Created($"{ApiRoutes.Documents}/{response.Id}", response);
        })
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .WithName("UploadDocument");

        group.MapPost("/", async (CreateDocumentRequest request, bool? index, HttpContext context, DocumentService service, CancellationToken cancellationToken) =>
        {
            var response = index == true
                ? await service.CreateAndIndexAsync(request, context.User.GetUserId(), cancellationToken)
                : await service.CreateAsync(request, context.User.GetUserId(), cancellationToken);

            return Results.Created($"{ApiRoutes.Documents}/{response.Id}", response);
        }).WithName("CreateDocument");

        group.MapGet("/", async (int? pageNumber, int? pageSize, string? tags, DocumentService service, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListAsync(pageNumber, pageSize, tags, cancellationToken));
        }).WithName("ListDocuments");

        group.MapGet("/{id:guid}", async (Guid id, DocumentService service, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetAsync(id, cancellationToken));
        }).WithName("GetDocument");

        group.MapPut("/{id:guid}", async (Guid id, UpdateDocumentRequest request, DocumentService service, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdateAsync(id, request, cancellationToken));
        }).WithName("UpdateDocument");

        group.MapDelete("/{id:guid}", async (Guid id, DocumentService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        }).WithName("DeleteDocument");

        group.MapPost("/{id:guid}/chunk", async (Guid id, DocumentService service, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ChunkAsync(id, cancellationToken));
        }).WithName("ChunkDocument");

        group.MapPost("/{id:guid}/embed", async (Guid id, DocumentService service, CancellationToken cancellationToken) =>
        {
            var generated = await service.EmbedAsync(id, cancellationToken);
            return Results.Ok(new { documentId = id, embeddingsGenerated = generated });
        }).WithName("EmbedDocument");

        group.MapPost("/{id:guid}/reindex", async (Guid id, DocumentService service, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ReindexAsync(id, cancellationToken));
        }).WithName("ReindexDocument");

        group.MapGet("/{id:guid}/chunks", async (Guid id, DocumentService service, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetChunksAsync(id, cancellationToken));
        }).WithName("GetDocumentChunks");
    }
}

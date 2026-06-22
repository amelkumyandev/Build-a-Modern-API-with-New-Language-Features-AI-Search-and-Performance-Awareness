using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Api;

public static class EvaluationEndpoints
{
    public static void MapEvaluationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Evaluation).RequireAuthorization().WithTags("Evaluation");

        group.MapPost("/generate-questions", async (EvaluationService evaluation, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await evaluation.GenerateQuestionsAsync(cancellationToken));
        }).WithName("GenerateEvaluationQuestions");

        group.MapGet("/questions", async (EvaluationService evaluation, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await evaluation.ListQuestionsAsync(cancellationToken));
        }).WithName("ListEvaluationQuestions");

        group.MapPost("/run", async (EvaluationRunRequest request, EvaluationService evaluation, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await evaluation.RunAsync(request, cancellationToken));
        }).WithName("RunEvaluation");

        group.MapGet("/runs/{id:guid}", async (Guid id, EvaluationService evaluation, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await evaluation.GetRunAsync(id, cancellationToken));
        }).WithName("GetEvaluationRun");
    }
}

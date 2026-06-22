using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Api;

public static class AgentEndpoints
{
    public static void MapAgentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Agent).RequireAuthorization().WithTags("Agent");

        group.MapPost("/chat", async (AgentChatRequest request, HttpContext context, AgentOrchestrator agent, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await agent.ChatAsync(context.User.GetUserId(), request, cancellationToken));
        }).WithName("AgentChat");

        group.MapGet("/sessions", async (HttpContext context, AgentOrchestrator agent, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await agent.ListSessionsAsync(context.User.GetUserId(), cancellationToken));
        }).WithName("ListAgentSessions");

        group.MapGet("/sessions/{id:guid}", async (Guid id, HttpContext context, AgentOrchestrator agent, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await agent.GetSessionAsync(id, context.User.GetUserId(), cancellationToken));
        }).WithName("GetAgentSession");

        group.MapGet("/runs/{id:guid}", async (Guid id, AgentOrchestrator agent, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await agent.GetRunAsync(id, cancellationToken));
        }).WithName("GetAgentRun");
    }
}

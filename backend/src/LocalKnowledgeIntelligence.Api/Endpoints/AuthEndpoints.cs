using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Api;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Auth).WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, AuthService auth, CancellationToken cancellationToken) =>
        {
            var response = await auth.LoginAsync(request, cancellationToken);
            return Results.Ok(response);
        }).AllowAnonymous().WithName("Login");

        group.MapPost("/logout", () => Results.NoContent()).RequireAuthorization().WithName("Logout");

        group.MapGet("/me", async (HttpContext context, AuthService auth, CancellationToken cancellationToken) =>
        {
            var response = await auth.GetMeAsync(context.User.GetUserId(), cancellationToken);
            return Results.Ok(response);
        }).RequireAuthorization().WithName("GetCurrentUser");
    }
}

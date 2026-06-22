using System.Security.Claims;

namespace LocalKnowledgeIntelligence.Application;

public static class CurrentUser
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedApplicationException("The current token does not contain a user id.");

        return Guid.TryParse(raw, out var id)
            ? id
            : throw new UnauthorizedApplicationException("The current token contains an invalid user id.");
    }
}

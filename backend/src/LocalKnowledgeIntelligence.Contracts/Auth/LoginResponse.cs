namespace LocalKnowledgeIntelligence.Contracts;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User);

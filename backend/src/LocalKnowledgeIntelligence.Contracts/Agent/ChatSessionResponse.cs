namespace LocalKnowledgeIntelligence.Contracts;

public sealed record ChatSessionResponse(Guid Id, string Title, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

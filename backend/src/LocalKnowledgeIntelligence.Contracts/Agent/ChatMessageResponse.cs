namespace LocalKnowledgeIntelligence.Contracts;

public sealed record ChatMessageResponse(Guid Id, Guid SessionId, string Role, string Content, IReadOnlyList<CitationResponse> Citations, DateTimeOffset CreatedAt);

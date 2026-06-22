namespace LocalKnowledgeIntelligence.Contracts;

public sealed record AgentRunResponse(Guid Id, Guid SessionId, string Status, string Model, string SearchMode, IReadOnlyList<AgentStepResponse> Steps, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt);

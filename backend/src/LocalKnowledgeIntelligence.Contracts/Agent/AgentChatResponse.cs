namespace LocalKnowledgeIntelligence.Contracts;

public sealed record AgentChatResponse(
    Guid SessionId,
    Guid MessageId,
    string Answer,
    IReadOnlyList<CitationResponse> Citations,
    Guid AgentRunId);

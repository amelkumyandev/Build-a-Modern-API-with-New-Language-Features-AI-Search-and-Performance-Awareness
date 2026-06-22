namespace LocalKnowledgeIntelligence.Contracts;

public sealed record AgentChatRequest(Guid? SessionId, string Message, string? SearchMode, int? TopK);

namespace LocalKnowledgeIntelligence.Contracts;

public sealed record ChatSessionDetailResponse(ChatSessionResponse Session, IReadOnlyList<ChatMessageResponse> Messages);

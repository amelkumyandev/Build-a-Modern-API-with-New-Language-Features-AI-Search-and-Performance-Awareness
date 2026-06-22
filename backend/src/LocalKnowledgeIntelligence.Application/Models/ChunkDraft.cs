namespace LocalKnowledgeIntelligence.Application;

public sealed record ChunkDraft(int Index, string Content, int TokenEstimate, Dictionary<string, object?> Metadata);

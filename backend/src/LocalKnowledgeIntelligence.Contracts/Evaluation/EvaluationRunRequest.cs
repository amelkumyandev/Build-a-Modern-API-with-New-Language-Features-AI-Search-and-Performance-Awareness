namespace LocalKnowledgeIntelligence.Contracts;

public sealed record EvaluationRunRequest(string? SearchMode, int? TopK);

namespace LocalKnowledgeIntelligence.Contracts;

public sealed record AgentStepResponse(Guid Id, int StepIndex, string ToolType, string Input, string Output, int DurationMs, DateTimeOffset CreatedAt);

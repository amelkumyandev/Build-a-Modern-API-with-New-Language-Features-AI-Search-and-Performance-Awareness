namespace LocalKnowledgeIntelligence.Contracts;

public sealed record EvaluationRunResponse(
    Guid Id,
    string SearchMode,
    int QuestionCount,
    double Score,
    IReadOnlyList<EvaluationQuestionResultResponse> Results,
    DateTimeOffset CreatedAt,
    DateTimeOffset CompletedAt);

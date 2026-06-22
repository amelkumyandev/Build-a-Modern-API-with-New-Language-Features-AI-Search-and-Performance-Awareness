namespace LocalKnowledgeIntelligence.Contracts;

public sealed record EvaluationQuestionResponse(
    Guid Id,
    string Question,
    IReadOnlyList<string> ExpectedAnswerKeywords,
    IReadOnlyList<string> ExpectedDocumentTitles,
    string Difficulty,
    DateTimeOffset CreatedAt);

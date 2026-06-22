namespace LocalKnowledgeIntelligence.Domain;

public sealed record EvaluationQuestionResult(
    Guid QuestionId,
    string Question,
    string Difficulty,
    IReadOnlyList<string> ExpectedDocumentTitles,
    IReadOnlyList<string> MatchedExpectedDocumentTitles,
    IReadOnlyList<string> MissingExpectedDocumentTitles,
    IReadOnlyList<string> ExpectedAnswerKeywords,
    IReadOnlyList<string> MatchedExpectedKeywords,
    IReadOnlyList<string> MissingExpectedKeywords,
    IReadOnlyList<EvaluationRetrievedChunk> RetrievedChunks,
    double Score,
    bool Passed);

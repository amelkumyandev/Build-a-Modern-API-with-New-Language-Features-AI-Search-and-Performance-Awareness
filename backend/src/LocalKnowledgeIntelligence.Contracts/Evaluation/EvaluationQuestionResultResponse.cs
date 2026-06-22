namespace LocalKnowledgeIntelligence.Contracts;

public sealed record EvaluationQuestionResultResponse(
    Guid QuestionId,
    string Question,
    string Difficulty,
    IReadOnlyList<string> ExpectedDocumentTitles,
    IReadOnlyList<string> MatchedExpectedDocumentTitles,
    IReadOnlyList<string> MissingExpectedDocumentTitles,
    IReadOnlyList<string> ExpectedAnswerKeywords,
    IReadOnlyList<string> MatchedExpectedKeywords,
    IReadOnlyList<string> MissingExpectedKeywords,
    IReadOnlyList<EvaluationRetrievedChunkResponse> RetrievedChunks,
    double Score,
    bool Passed);

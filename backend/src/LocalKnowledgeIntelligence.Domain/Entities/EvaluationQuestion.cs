namespace LocalKnowledgeIntelligence.Domain;

public sealed class EvaluationQuestion
{
    private EvaluationQuestion()
    {
    }

    private EvaluationQuestion(Guid id, string question, IReadOnlyList<string> expectedAnswerKeywords, IReadOnlyList<string> expectedDocumentTitles, string difficulty, DateTimeOffset now)
    {
        Id = id;
        Question = question;
        ExpectedAnswerKeywords = [.. expectedAnswerKeywords];
        ExpectedDocumentTitles = [.. expectedDocumentTitles];
        Difficulty = difficulty;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public List<string> ExpectedAnswerKeywords { get; private set; } = [];
    public List<string> ExpectedDocumentTitles { get; private set; } = [];
    public string Difficulty { get; private set; } = "medium";
    public DateTimeOffset CreatedAt { get; private set; }

    public static EvaluationQuestion Create(string question, IReadOnlyList<string> expectedAnswerKeywords, IReadOnlyList<string> expectedDocumentTitles, string difficulty, DateTimeOffset now)
    {
        return new EvaluationQuestion(Guid.NewGuid(), question, expectedAnswerKeywords, expectedDocumentTitles, difficulty, now);
    }
}

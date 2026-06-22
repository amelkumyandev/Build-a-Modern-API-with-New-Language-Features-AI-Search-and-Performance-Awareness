namespace LocalKnowledgeIntelligence.Domain;

public sealed class EvaluationRun
{
    private EvaluationRun()
    {
    }

    private EvaluationRun(Guid id, SearchMode searchMode, int questionCount, double score, IReadOnlyList<EvaluationQuestionResult> results, DateTimeOffset now)
    {
        Id = id;
        SearchMode = searchMode;
        QuestionCount = questionCount;
        Score = score;
        Results = [.. results];
        CreatedAt = now;
        CompletedAt = now;
    }

    public Guid Id { get; private set; }
    public SearchMode SearchMode { get; private set; }
    public int QuestionCount { get; private set; }
    public double Score { get; private set; }
    public List<EvaluationQuestionResult> Results { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }

    public static EvaluationRun Complete(SearchMode searchMode, int questionCount, double score, IReadOnlyList<EvaluationQuestionResult>? results, DateTimeOffset now)
    {
        return new EvaluationRun(Guid.NewGuid(), searchMode, questionCount, Math.Clamp(score, 0, 1), results ?? [], now);
    }
}

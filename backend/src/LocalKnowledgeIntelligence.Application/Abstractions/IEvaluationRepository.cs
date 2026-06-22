using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public interface IEvaluationRepository
{
    Task<int> AddQuestionsIfMissingAsync(IReadOnlyList<EvaluationQuestion> questions, CancellationToken cancellationToken);
    Task<IReadOnlyList<EvaluationQuestion>> ListQuestionsAsync(CancellationToken cancellationToken);
    Task AddRunAsync(EvaluationRun run, CancellationToken cancellationToken);
    Task<EvaluationRun?> GetRunAsync(Guid id, CancellationToken cancellationToken);
    Task<EvaluationRun?> GetLatestRunAsync(CancellationToken cancellationToken);
}

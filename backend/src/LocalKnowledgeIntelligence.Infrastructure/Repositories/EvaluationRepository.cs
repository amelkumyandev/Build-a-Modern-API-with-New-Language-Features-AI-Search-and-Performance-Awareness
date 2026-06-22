using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class EvaluationRepository(AppDbContext db) : IEvaluationRepository
{
    public async Task<int> AddQuestionsIfMissingAsync(IReadOnlyList<EvaluationQuestion> questions, CancellationToken cancellationToken)
    {
        var existing = await db.EvaluationQuestions.Select(question => question.Question).ToArrayAsync(cancellationToken);
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newQuestions = questions.Where(question => !set.Contains(question.Question)).ToArray();
        db.EvaluationQuestions.AddRange(newQuestions);
        await db.SaveChangesAsync(cancellationToken);
        return newQuestions.Length;
    }

    public async Task<IReadOnlyList<EvaluationQuestion>> ListQuestionsAsync(CancellationToken cancellationToken)
    {
        return await db.EvaluationQuestions.AsNoTracking().OrderBy(question => question.Question).ToArrayAsync(cancellationToken);
    }

    public async Task AddRunAsync(EvaluationRun run, CancellationToken cancellationToken)
    {
        db.EvaluationRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<EvaluationRun?> GetRunAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.EvaluationRuns.AsNoTracking().FirstOrDefaultAsync(run => run.Id == id, cancellationToken);
    }

    public Task<EvaluationRun?> GetLatestRunAsync(CancellationToken cancellationToken)
    {
        return db.EvaluationRuns.AsNoTracking().OrderByDescending(run => run.CompletedAt).FirstOrDefaultAsync(cancellationToken);
    }
}

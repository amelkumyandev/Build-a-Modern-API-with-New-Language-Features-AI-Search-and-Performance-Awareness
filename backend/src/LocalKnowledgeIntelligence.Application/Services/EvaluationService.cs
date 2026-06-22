using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class EvaluationService(IEvaluationRepository evaluations, SearchService search, EvaluationScorer scorer, IClock clock)
{
    public async Task<GenerateQuestionsResponse> GenerateQuestionsAsync(CancellationToken cancellationToken)
    {
        var created = await evaluations.AddQuestionsIfMissingAsync(SeedCatalog.Questions(clock.UtcNow), cancellationToken);
        return new GenerateQuestionsResponse(created);
    }

    public async Task<IReadOnlyList<EvaluationQuestionResponse>> ListQuestionsAsync(CancellationToken cancellationToken)
    {
        var questions = await evaluations.ListQuestionsAsync(cancellationToken);
        return questions.Select(question => question.ToResponse()).ToArray();
    }

    public async Task<EvaluationRunResponse> RunAsync(EvaluationRunRequest request, CancellationToken cancellationToken)
    {
        var mode = DocumentValidator.ParseSearchMode(request.SearchMode);
        var topK = Math.Clamp(request.TopK ?? 8, 1, 20);
        var questions = await evaluations.ListQuestionsAsync(cancellationToken);

        if (questions.Count == 0)
        {
            throw ValidationFailureException.For("questions", "Generate evaluation questions before running evaluation.");
        }

        var results = new List<EvaluationQuestionResult>();
        foreach (var question in questions)
        {
            var response = await search.SearchAsync(question.Question, mode, topK, cancellationToken);
            var hits = response.Items.Select(item => new SearchHit(
                item.DocumentId,
                item.ChunkId,
                item.Title,
                item.Snippet,
                item.VectorScore,
                item.KeywordScore,
                item.RecencyScore,
                item.FinalScore,
                item.Distance,
                item.MatchedFields,
                clock.UtcNow)).ToArray();
            results.Add(BuildQuestionResult(question, hits, scorer.Score(question, hits)));
        }

        var score = results.Count == 0 ? 0 : results.Average(result => result.Score);
        var run = EvaluationRun.Complete(mode, questions.Count, score, results, clock.UtcNow);
        await evaluations.AddRunAsync(run, cancellationToken);
        return run.ToResponse();
    }

    public async Task<EvaluationRunResponse> GetRunAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await evaluations.GetRunAsync(id, cancellationToken)
            ?? throw new NotFoundException("Evaluation run was not found.");

        return run.ToResponse();
    }

    private static EvaluationQuestionResult BuildQuestionResult(EvaluationQuestion question, IReadOnlyList<SearchHit> hits, double score)
    {
        var matchedDocuments = question.ExpectedDocumentTitles
            .Where(expected => hits.Any(hit => TitleMatches(expected, hit.Title)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var missingDocuments = question.ExpectedDocumentTitles
            .Except(matchedDocuments, StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var matchedKeywords = question.ExpectedAnswerKeywords
            .Where(keyword => hits.Any(hit => ContainsKeyword(hit, keyword)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var missingKeywords = question.ExpectedAnswerKeywords
            .Except(matchedKeywords, StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var retrievedChunks = hits
            .Take(5)
            .Select(hit => new EvaluationRetrievedChunk(
                hit.DocumentId,
                hit.ChunkId,
                hit.Title,
                hit.Snippet,
                hit.FinalScore,
                hit.VectorScore,
                hit.KeywordScore))
            .ToArray();

        return new EvaluationQuestionResult(
            question.Id,
            question.Question,
            question.Difficulty,
            question.ExpectedDocumentTitles,
            matchedDocuments,
            missingDocuments,
            question.ExpectedAnswerKeywords,
            matchedKeywords,
            missingKeywords,
            retrievedChunks,
            score,
            missingDocuments.Length == 0);
    }

    private static bool TitleMatches(string expected, string actual)
    {
        return actual.Contains(expected, StringComparison.OrdinalIgnoreCase)
            || expected.Contains(actual, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsKeyword(SearchHit hit, string keyword)
    {
        return hit.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || hit.Snippet.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || hit.MatchedFields.Any(field => field.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}

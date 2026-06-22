using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class EvaluationScorer
{
    public double Score(EvaluationQuestion question, IReadOnlyList<SearchHit> hits)
    {
        if (question.ExpectedDocumentTitles.Count == 0)
        {
            return 0;
        }

        var matched = question.ExpectedDocumentTitles.Count(expected =>
            hits.Any(hit => hit.Title.Contains(expected, StringComparison.OrdinalIgnoreCase)
                || expected.Contains(hit.Title, StringComparison.OrdinalIgnoreCase)));

        return Math.Clamp(matched / (double)question.ExpectedDocumentTitles.Count, 0, 1);
    }
}

namespace LocalKnowledgeIntelligence.Application;

public sealed class HybridScoreCalculator(SearchOptions options)
{
    public double Calculate(double vectorScore, double keywordScore, double recencyScore)
    {
        return Math.Clamp(
            options.VectorWeight * vectorScore +
            options.KeywordWeight * keywordScore +
            options.RecencyWeight * recencyScore,
            0,
            1);
    }

    public double NormalizeKeywordScore(double score, double maxScore)
    {
        return maxScore <= 0 ? 0 : Math.Clamp(score / maxScore, 0, 1);
    }

    public double CalculateRecencyScore(DateTimeOffset updatedAt, DateTimeOffset now)
    {
        var days = Math.Max(0, (now - updatedAt).TotalDays);
        return Math.Clamp(1.0 / (1.0 + days / 30.0), 0, 1);
    }
}

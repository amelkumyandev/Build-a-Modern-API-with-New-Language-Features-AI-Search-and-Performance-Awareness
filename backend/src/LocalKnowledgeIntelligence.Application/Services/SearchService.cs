using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class SearchService(ISearchRepository search, IEmbeddingClient embeddings, RuntimeSettings settings, IClock clock)
{
    public async Task<SearchResponse> KeywordAsync(string query, int? limit, CancellationToken cancellationToken)
    {
        var normalizedLimit = NormalizeLimit(limit);
        DocumentValidator.ValidateQuery(query, normalizedLimit, settings.MaxSearchLimit);

        var hits = await search.KeywordSearchAsync(query.Trim(), normalizedLimit, cancellationToken);
        return new SearchResponse(query.Trim(), hits.Select(hit => hit.ToResponse()).ToArray());
    }

    public async Task<SearchResponse> SemanticAsync(string query, int? limit, CancellationToken cancellationToken)
    {
        var normalizedLimit = NormalizeLimit(limit);
        DocumentValidator.ValidateQuery(query, normalizedLimit, settings.MaxSearchLimit);

        var vector = await embeddings.GenerateEmbeddingAsync(query.Trim(), cancellationToken);
        var hits = await search.SemanticSearchAsync(vector, normalizedLimit, cancellationToken);
        return new SearchResponse(query.Trim(), hits.Select(hit => hit.ToResponse()).ToArray());
    }

    public async Task<SearchResponse> HybridAsync(string query, int? limit, CancellationToken cancellationToken)
    {
        var normalizedLimit = NormalizeLimit(limit);
        DocumentValidator.ValidateQuery(query, normalizedLimit, settings.MaxSearchLimit);

        var options = settings.SearchOptions;
        var calculator = new HybridScoreCalculator(options);
        var vector = await embeddings.GenerateEmbeddingAsync(query.Trim(), cancellationToken);
        var semanticHits = await search.SemanticSearchAsync(vector, normalizedLimit * 2, cancellationToken);
        var keywordHits = await search.KeywordSearchAsync(query.Trim(), normalizedLimit * 2, cancellationToken);
        var maxKeywordScore = keywordHits.Count == 0 ? 0 : keywordHits.Max(hit => hit.KeywordScore);

        var combined = semanticHits
            .Concat(keywordHits)
            .GroupBy(hit => hit.ChunkId)
            .Select(group =>
            {
                var best = group.OrderByDescending(hit => hit.FinalScore).First();
                var vectorScore = group.Max(hit => hit.VectorScore);
                var keywordScore = calculator.NormalizeKeywordScore(group.Max(hit => hit.KeywordScore), maxKeywordScore);
                var recencyScore = calculator.CalculateRecencyScore(best.UpdatedAt, clock.UtcNow);
                var finalScore = calculator.Calculate(vectorScore, keywordScore, recencyScore);
                var matchedFields = group.SelectMany(hit => hit.MatchedFields).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();

                return best with
                {
                    VectorScore = vectorScore,
                    KeywordScore = keywordScore,
                    RecencyScore = recencyScore,
                    FinalScore = finalScore,
                    MatchedFields = matchedFields
                };
            })
            .OrderByDescending(hit => hit.FinalScore)
            .ThenBy(hit => hit.Distance ?? double.MaxValue)
            .Take(normalizedLimit)
            .ToArray();

        return new SearchResponse(query.Trim(), combined.Select(hit => hit.ToResponse()).ToArray());
    }

    public Task<SearchResponse> SearchAsync(string query, SearchMode mode, int? limit, CancellationToken cancellationToken)
    {
        return mode switch
        {
            SearchMode.Keyword => KeywordAsync(query, limit, cancellationToken),
            SearchMode.Semantic => SemanticAsync(query, limit, cancellationToken),
            _ => HybridAsync(query, limit, cancellationToken)
        };
    }

    private int NormalizeLimit(int? limit)
    {
        return Math.Clamp(limit ?? settings.DefaultSearchLimit, 1, settings.MaxSearchLimit);
    }
}

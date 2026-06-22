namespace LocalKnowledgeIntelligence.Application;

public sealed record SearchHit(
    Guid DocumentId,
    Guid ChunkId,
    string Title,
    string Snippet,
    double VectorScore,
    double KeywordScore,
    double RecencyScore,
    double FinalScore,
    double? Distance,
    IReadOnlyList<string> MatchedFields,
    DateTimeOffset UpdatedAt);

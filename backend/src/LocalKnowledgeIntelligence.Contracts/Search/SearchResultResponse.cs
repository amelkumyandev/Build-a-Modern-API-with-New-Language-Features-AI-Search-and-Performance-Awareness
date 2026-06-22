namespace LocalKnowledgeIntelligence.Contracts;

public sealed record SearchResultResponse(
    Guid DocumentId,
    Guid ChunkId,
    string Title,
    string Snippet,
    double VectorScore,
    double KeywordScore,
    double RecencyScore,
    double FinalScore,
    double? Distance,
    IReadOnlyList<string> MatchedFields);

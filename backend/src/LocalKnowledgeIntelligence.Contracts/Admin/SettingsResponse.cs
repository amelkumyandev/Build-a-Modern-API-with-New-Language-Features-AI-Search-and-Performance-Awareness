namespace LocalKnowledgeIntelligence.Contracts;

public sealed record SettingsResponse(
    string EmbeddingModel,
    string ChatModel,
    int EmbeddingDimensions,
    int TargetTokenCount,
    int OverlapTokenCount,
    int DefaultSearchLimit,
    int MaxSearchLimit,
    double VectorWeight,
    double KeywordWeight,
    double RecencyWeight);

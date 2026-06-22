namespace LocalKnowledgeIntelligence.Contracts;

public sealed record UpdateSettingsRequest(
    string? ChatModel,
    int? DefaultSearchLimit,
    double? VectorWeight,
    double? KeywordWeight,
    double? RecencyWeight);

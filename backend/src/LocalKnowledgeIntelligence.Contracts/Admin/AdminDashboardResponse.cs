namespace LocalKnowledgeIntelligence.Contracts;

public sealed record AdminDashboardResponse(
    bool DatabaseReady,
    bool OpenAiKeyConfigured,
    int TotalDocuments,
    int TotalChunks,
    int EmbeddedChunks,
    int FailedEmbeddings,
    double? LatestEvaluationScore,
    string DevelopmentWarning);

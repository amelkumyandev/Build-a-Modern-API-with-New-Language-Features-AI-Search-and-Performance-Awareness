namespace LocalKnowledgeIntelligence.Contracts;

public sealed record EvaluationRetrievedChunkResponse(
    Guid DocumentId,
    Guid ChunkId,
    string Title,
    string Snippet,
    double FinalScore,
    double VectorScore,
    double KeywordScore);

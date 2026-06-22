namespace LocalKnowledgeIntelligence.Domain;

public sealed record EvaluationRetrievedChunk(
    Guid DocumentId,
    Guid ChunkId,
    string Title,
    string Snippet,
    double FinalScore,
    double VectorScore,
    double KeywordScore);

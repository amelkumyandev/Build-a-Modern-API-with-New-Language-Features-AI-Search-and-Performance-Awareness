using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public static class ContractMapping
{
    public static UserResponse ToResponse(this User user) => new(user.Id, user.Username, user.Role.ToString());

    public static DocumentSummaryResponse ToSummaryResponse(this KnowledgeDocument document)
    {
        return new(
            document.Id,
            document.Title,
            document.Summary,
            document.Tags,
            document.Status.ToString(),
            document.ChunkingStatus.ToString(),
            document.EmbeddingStatus.ToString(),
            document.CreatedAt,
            document.UpdatedAt);
    }

    public static DocumentDetailResponse ToDetailResponse(this KnowledgeDocument document)
    {
        return new(
            document.Id,
            document.Title,
            document.Content,
            document.Summary,
            document.Tags,
            document.Metadata,
            document.Status.ToString(),
            document.ChunkingStatus.ToString(),
            document.EmbeddingStatus.ToString(),
            document.CreatedByUserId,
            document.CreatedAt,
            document.UpdatedAt);
    }

    public static ChunkResponse ToResponse(this DocumentChunk chunk)
    {
        return new(
            chunk.Id,
            chunk.DocumentId,
            chunk.ChunkIndex,
            chunk.Content,
            chunk.TokenEstimate,
            chunk.Metadata,
            chunk.EmbeddingModel,
            chunk.EmbeddingDimensions,
            chunk.EmbeddingGeneratedAt);
    }

    public static SearchResultResponse ToResponse(this SearchHit hit)
    {
        return new(
            hit.DocumentId,
            hit.ChunkId,
            hit.Title,
            hit.Snippet,
            Math.Round(hit.VectorScore, 4),
            Math.Round(hit.KeywordScore, 4),
            Math.Round(hit.RecencyScore, 4),
            Math.Round(hit.FinalScore, 4),
            hit.Distance is null ? null : Math.Round(hit.Distance.Value, 4),
            hit.MatchedFields);
    }

    public static CitationResponse ToResponse(this Citation citation)
    {
        return new(citation.DocumentId, citation.ChunkId, citation.Title, citation.Snippet, Math.Round(citation.Score, 4));
    }

    public static ChatSessionResponse ToResponse(this ChatSession session)
    {
        return new(session.Id, session.Title, session.CreatedAt, session.UpdatedAt);
    }

    public static ChatMessageResponse ToResponse(this ChatMessage message)
    {
        return new(message.Id, message.SessionId, message.Role.ToString(), message.Content, message.Citations.Select(c => c.ToResponse()).ToArray(), message.CreatedAt);
    }

    public static AgentRunResponse ToResponse(this AgentRun run)
    {
        return new(
            run.Id,
            run.SessionId,
            run.Status.ToString(),
            run.Model,
            run.SearchMode.ToString(),
            run.Steps.OrderBy(step => step.StepIndex).Select(step => step.ToResponse()).ToArray(),
            run.CreatedAt,
            run.CompletedAt);
    }

    public static AgentStepResponse ToResponse(this AgentStep step)
    {
        return new(step.Id, step.StepIndex, step.ToolType.ToString(), step.Input, step.Output, step.DurationMs, step.CreatedAt);
    }

    public static EvaluationQuestionResponse ToResponse(this EvaluationQuestion question)
    {
        return new(question.Id, question.Question, question.ExpectedAnswerKeywords, question.ExpectedDocumentTitles, question.Difficulty, question.CreatedAt);
    }

    public static EvaluationRunResponse ToResponse(this EvaluationRun run)
    {
        return new(
            run.Id,
            run.SearchMode.ToString(),
            run.QuestionCount,
            Math.Round(run.Score, 4),
            run.Results.Select(result => result.ToResponse()).ToArray(),
            run.CreatedAt,
            run.CompletedAt);
    }

    public static EvaluationQuestionResultResponse ToResponse(this EvaluationQuestionResult result)
    {
        return new(
            result.QuestionId,
            result.Question,
            result.Difficulty,
            result.ExpectedDocumentTitles,
            result.MatchedExpectedDocumentTitles,
            result.MissingExpectedDocumentTitles,
            result.ExpectedAnswerKeywords,
            result.MatchedExpectedKeywords,
            result.MissingExpectedKeywords,
            result.RetrievedChunks.Select(chunk => chunk.ToResponse()).ToArray(),
            Math.Round(result.Score, 4),
            result.Passed);
    }

    public static EvaluationRetrievedChunkResponse ToResponse(this EvaluationRetrievedChunk chunk)
    {
        return new(
            chunk.DocumentId,
            chunk.ChunkId,
            chunk.Title,
            chunk.Snippet,
            Math.Round(chunk.FinalScore, 4),
            Math.Round(chunk.VectorScore, 4),
            Math.Round(chunk.KeywordScore, 4));
    }
}

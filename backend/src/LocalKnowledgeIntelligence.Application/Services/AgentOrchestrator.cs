using System.Diagnostics;
using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class AgentOrchestrator(
    IAgentRepository repository,
    SearchService searchService,
    IChatCompletionClient chat,
    AgentPromptBuilder promptBuilder,
    RuntimeSettings settings,
    IClock clock)
{
    public async Task<AgentChatResponse> ChatAsync(Guid userId, AgentChatRequest request, CancellationToken cancellationToken)
    {
        var topK = Math.Clamp(request.TopK ?? 8, 1, 20);
        var mode = DocumentValidator.ParseSearchMode(request.SearchMode);
        DocumentValidator.ValidateAgentMessage(request, topK);

        var session = request.SessionId is Guid sessionId
            ? await repository.GetSessionAsync(sessionId, userId, cancellationToken) ?? throw new NotFoundException("Chat session was not found.")
            : ChatSession.Create(CreateTitle(request.Message), userId, clock.UtcNow);

        if (request.SessionId is null)
        {
            await repository.AddSessionAsync(session, cancellationToken);
        }

        var userMessage = ChatMessage.Create(session.Id, ChatMessageRole.User, request.Message.Trim(), [], clock.UtcNow);
        await repository.AddMessageAsync(userMessage, cancellationToken);

        var run = AgentRun.Start(session.Id, userMessage.Id, settings.ChatModel, mode, clock.UtcNow);
        await repository.AddRunAsync(run, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var retrievalStopwatch = Stopwatch.StartNew();
        var searchResponse = await searchService.SearchAsync(request.Message.Trim(), mode, topK, cancellationToken);
        retrievalStopwatch.Stop();

        var hits = searchResponse.Items.Select(item => new SearchHit(
            item.DocumentId,
            item.ChunkId,
            item.Title,
            item.Snippet,
            item.VectorScore,
            item.KeywordScore,
            item.RecencyScore,
            item.FinalScore,
            item.Distance,
            item.MatchedFields,
            clock.UtcNow)).ToArray();

        await repository.AddStepAsync(AgentStep.Create(run.Id, 1, AgentToolType.Retrieval, request.Message, $"Retrieved {hits.Length} chunks.", retrievalStopwatch.Elapsed, clock.UtcNow), cancellationToken);

        var citations = hits
            .Take(topK)
            .Select(hit => new Citation(hit.DocumentId, hit.ChunkId, hit.Title, hit.Snippet, hit.FinalScore))
            .ToArray();

        var prompt = promptBuilder.Build(request.Message, hits);
        var answerStopwatch = Stopwatch.StartNew();
        var answer = hits.Length == 0
            ? new GeneratedAnswer("I could not find enough local context to answer that question with citations.")
            : await chat.GenerateAnswerAsync(prompt, cancellationToken);
        answerStopwatch.Stop();

        await repository.AddStepAsync(AgentStep.Create(run.Id, 2, AgentToolType.AnswerGeneration, "Grounded answer prompt", answer.Answer, answerStopwatch.Elapsed, clock.UtcNow), cancellationToken);
        await repository.AddStepAsync(AgentStep.Create(run.Id, 3, AgentToolType.Citation, "Retrieved chunks", $"Created {citations.Length} citations.", TimeSpan.Zero, clock.UtcNow), cancellationToken);

        var assistantMessage = ChatMessage.Create(session.Id, ChatMessageRole.Assistant, answer.Answer, citations, clock.UtcNow);
        await repository.AddMessageAsync(assistantMessage, cancellationToken);
        session.Touch(clock.UtcNow);
        run.Complete(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return new AgentChatResponse(session.Id, assistantMessage.Id, assistantMessage.Content, citations.Select(citation => citation.ToResponse()).ToArray(), run.Id);
    }

    public async Task<IReadOnlyList<ChatSessionResponse>> ListSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await repository.ListSessionsAsync(userId, cancellationToken);
        return sessions.Select(session => session.ToResponse()).ToArray();
    }

    public async Task<ChatSessionDetailResponse> GetSessionAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var session = await repository.GetSessionAsync(id, userId, cancellationToken)
            ?? throw new NotFoundException("Chat session was not found.");

        var messages = await repository.GetMessagesAsync(id, cancellationToken);
        return new ChatSessionDetailResponse(session.ToResponse(), messages.Select(message => message.ToResponse()).ToArray());
    }

    public async Task<AgentRunResponse> GetRunAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await repository.GetRunAsync(id, cancellationToken)
            ?? throw new NotFoundException("Agent run was not found.");

        return run.ToResponse();
    }

    private static string CreateTitle(string message)
    {
        var trimmed = message.Trim();
        return trimmed.Length <= 60 ? trimmed : $"{trimmed[..60]}...";
    }
}

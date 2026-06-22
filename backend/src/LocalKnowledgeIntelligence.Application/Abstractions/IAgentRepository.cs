using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public interface IAgentRepository
{
    Task<ChatSession?> GetSessionAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatSession>> ListSessionsAsync(Guid userId, CancellationToken cancellationToken);
    Task AddSessionAsync(ChatSession session, CancellationToken cancellationToken);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken);
    Task AddRunAsync(AgentRun run, CancellationToken cancellationToken);
    Task<AgentRun?> GetRunAsync(Guid runId, CancellationToken cancellationToken);
    Task AddStepAsync(AgentStep step, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

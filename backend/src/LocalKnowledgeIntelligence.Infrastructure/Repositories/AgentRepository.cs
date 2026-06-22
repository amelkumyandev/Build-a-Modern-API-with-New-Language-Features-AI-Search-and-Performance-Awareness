using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class AgentRepository(AppDbContext db) : IAgentRepository
{
    public Task<ChatSession?> GetSessionAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return db.ChatSessions.FirstOrDefaultAsync(session => session.Id == id && session.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatSession>> ListSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.ChatSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.UpdatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddSessionAsync(ChatSession session, CancellationToken cancellationToken)
    {
        db.ChatSessions.Add(session);
        return Task.CompletedTask;
    }

    public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        db.ChatMessages.Add(message);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await db.ChatMessages
            .AsNoTracking()
            .Where(message => message.SessionId == sessionId)
            .OrderBy(message => message.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddRunAsync(AgentRun run, CancellationToken cancellationToken)
    {
        db.AgentRuns.Add(run);
        return Task.CompletedTask;
    }

    public Task<AgentRun?> GetRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        return db.AgentRuns.Include(run => run.Steps).FirstOrDefaultAsync(run => run.Id == runId, cancellationToken);
    }

    public Task AddStepAsync(AgentStep step, CancellationToken cancellationToken)
    {
        db.AgentSteps.Add(step);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

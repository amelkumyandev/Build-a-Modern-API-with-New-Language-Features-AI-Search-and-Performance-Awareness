namespace LocalKnowledgeIntelligence.Domain;

public sealed class AgentRun
{
    private AgentRun()
    {
    }

    private AgentRun(Guid id, Guid sessionId, Guid userMessageId, string model, SearchMode searchMode, DateTimeOffset now)
    {
        Id = id;
        SessionId = sessionId;
        UserMessageId = userMessageId;
        Status = AgentRunStatus.Running;
        Model = model;
        SearchMode = searchMode;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid UserMessageId { get; private set; }
    public AgentRunStatus Status { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public SearchMode SearchMode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public List<AgentStep> Steps { get; private set; } = [];

    public static AgentRun Start(Guid sessionId, Guid userMessageId, string model, SearchMode searchMode, DateTimeOffset now)
    {
        return new AgentRun(Guid.NewGuid(), sessionId, userMessageId, model, searchMode, now);
    }

    public void Complete(DateTimeOffset now)
    {
        Status = AgentRunStatus.Completed;
        CompletedAt = now;
    }

    public void Fail(DateTimeOffset now)
    {
        Status = AgentRunStatus.Failed;
        CompletedAt = now;
    }
}

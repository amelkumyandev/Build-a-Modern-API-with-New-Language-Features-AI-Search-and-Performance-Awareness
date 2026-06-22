namespace LocalKnowledgeIntelligence.Domain;

public sealed class AgentStep
{
    private AgentStep()
    {
    }

    private AgentStep(Guid id, Guid agentRunId, int stepIndex, AgentToolType toolType, string input, string output, int durationMs, DateTimeOffset now)
    {
        Id = id;
        AgentRunId = agentRunId;
        StepIndex = stepIndex;
        ToolType = toolType;
        Input = input;
        Output = output;
        DurationMs = durationMs;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid AgentRunId { get; private set; }
    public int StepIndex { get; private set; }
    public AgentToolType ToolType { get; private set; }
    public string Input { get; private set; } = string.Empty;
    public string Output { get; private set; } = string.Empty;
    public int DurationMs { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static AgentStep Create(Guid runId, int stepIndex, AgentToolType toolType, string input, string output, TimeSpan duration, DateTimeOffset now)
    {
        return new AgentStep(Guid.NewGuid(), runId, stepIndex, toolType, input, output, (int)duration.TotalMilliseconds, now);
    }
}

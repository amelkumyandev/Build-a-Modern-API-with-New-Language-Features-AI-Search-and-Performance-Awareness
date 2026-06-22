namespace LocalKnowledgeIntelligence.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

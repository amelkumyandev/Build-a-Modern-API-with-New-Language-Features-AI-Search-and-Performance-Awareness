using LocalKnowledgeIntelligence.Application;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

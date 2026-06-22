namespace LocalKnowledgeIntelligence.Domain;

public sealed class ChatMessage
{
    private ChatMessage()
    {
    }

    private ChatMessage(Guid id, Guid sessionId, ChatMessageRole role, string content, IReadOnlyList<Citation> citations, DateTimeOffset now)
    {
        Id = id;
        SessionId = sessionId;
        Role = role;
        Content = content;
        Citations = [.. citations];
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public ChatMessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public List<Citation> Citations { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }

    public static ChatMessage Create(Guid sessionId, ChatMessageRole role, string content, IReadOnlyList<Citation>? citations, DateTimeOffset now)
    {
        return new ChatMessage(Guid.NewGuid(), sessionId, role, content, citations ?? [], now);
    }
}

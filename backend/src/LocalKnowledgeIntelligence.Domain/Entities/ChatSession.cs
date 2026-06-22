namespace LocalKnowledgeIntelligence.Domain;

public sealed class ChatSession
{
    private ChatSession()
    {
    }

    private ChatSession(Guid id, string title, Guid userId, DateTimeOffset now)
    {
        Id = id;
        Title = title;
        UserId = userId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ChatSession Create(string title, Guid userId, DateTimeOffset now)
    {
        return new ChatSession(Guid.NewGuid(), title.Trim().Length == 0 ? "New agent chat" : title.Trim(), userId, now);
    }

    public void Touch(DateTimeOffset now) => UpdatedAt = now;
}

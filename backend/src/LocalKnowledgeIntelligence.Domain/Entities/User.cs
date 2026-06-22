namespace LocalKnowledgeIntelligence.Domain;

public sealed class User
{
    private User()
    {
    }

    private User(Guid id, string username, string passwordHash, UserRole role, DateTimeOffset now)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static User CreateAdmin(string username, string passwordHash, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(Guid.NewGuid(), username.Trim(), passwordHash, UserRole.Admin, now);
    }
}

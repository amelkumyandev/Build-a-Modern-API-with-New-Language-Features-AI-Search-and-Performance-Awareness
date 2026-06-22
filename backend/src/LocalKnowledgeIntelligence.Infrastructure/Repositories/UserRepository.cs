using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return db.Users.FirstOrDefaultAsync(user => user.Username == username, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken)
    {
        return db.Users.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}

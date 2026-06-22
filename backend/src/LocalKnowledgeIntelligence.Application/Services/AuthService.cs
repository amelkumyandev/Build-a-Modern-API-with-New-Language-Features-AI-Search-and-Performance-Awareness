using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Application;

public sealed class AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService, IClock clock)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw ValidationFailureException.For("credentials", "Username and password are required.");
        }

        var user = await users.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedApplicationException("Invalid username or password.");
        }

        return tokenService.CreateToken(user, clock.UtcNow);
    }

    public async Task<UserResponse> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        return user.ToResponse();
    }
}

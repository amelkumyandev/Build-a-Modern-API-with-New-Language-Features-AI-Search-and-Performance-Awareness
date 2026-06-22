using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public interface ITokenService
{
    LoginResponse CreateToken(User user, DateTimeOffset now);
}

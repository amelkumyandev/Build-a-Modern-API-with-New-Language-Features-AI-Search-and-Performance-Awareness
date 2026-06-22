using System.Security.Cryptography;
using System.Text;
using LocalKnowledgeIntelligence.Application;
using Microsoft.IdentityModel.Tokens;

namespace LocalKnowledgeIntelligence.Infrastructure;

public static class JwtSigningKey
{
    public static SymmetricSecurityKey Create(string signingKey)
    {
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new ConfigurationException("JWT signing key is not configured.");
        }

        return new SymmetricSecurityKey(SHA256.HashData(Encoding.UTF8.GetBytes(signingKey)));
    }
}

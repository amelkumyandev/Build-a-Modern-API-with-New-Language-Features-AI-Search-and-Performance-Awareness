using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public LoginResponse CreateToken(User user, DateTimeOffset now)
    {
        var expires = now.AddHours(Math.Max(1, _options.ExpirationHours));
        var key = JwtSigningKey.Create(_options.SigningKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now.UtcDateTime,
            expires.UtcDateTime,
            credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user.ToResponse());
    }
}

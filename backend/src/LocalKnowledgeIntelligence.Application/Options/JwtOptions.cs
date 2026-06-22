namespace LocalKnowledgeIntelligence.Application;

public sealed class JwtOptions
{
    public string Issuer
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? "LocalKnowledgeIntelligence" : value.Trim();
    } = "LocalKnowledgeIntelligence";

    public string Audience
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? "LocalKnowledgeIntelligence.Admin" : value.Trim();
    } = "LocalKnowledgeIntelligence.Admin";

    public int ExpirationHours
    {
        get;
        set => field = Math.Max(1, value);
    } = 8;

    public string SigningKey
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? "local-dev-signing-key-change-me" : value.Trim();
    } = "local-dev-signing-key-change-me";
}

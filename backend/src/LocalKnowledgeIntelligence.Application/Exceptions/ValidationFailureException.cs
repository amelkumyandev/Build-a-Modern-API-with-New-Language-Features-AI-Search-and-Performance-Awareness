namespace LocalKnowledgeIntelligence.Application;

public sealed class ValidationFailureException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;

    public static ValidationFailureException For(string key, params ReadOnlySpan<string> messages)
    {
        return new ValidationFailureException(new Dictionary<string, string[]>
        {
            [key] = messages.ToArray()
        });
    }
}

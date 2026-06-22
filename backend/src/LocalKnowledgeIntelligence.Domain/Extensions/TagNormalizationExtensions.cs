namespace LocalKnowledgeIntelligence.Domain;

public static class TagNormalizationExtensions
{
    public static IReadOnlyList<string> NormalizeTags(this IEnumerable<string>? tags)
    {
        return tags?
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.Ordinal)
            .ToArray() ?? [];
    }
}

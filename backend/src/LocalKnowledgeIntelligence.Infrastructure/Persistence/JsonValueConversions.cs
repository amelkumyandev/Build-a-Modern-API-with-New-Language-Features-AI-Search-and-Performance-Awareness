using System.Text.Json;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LocalKnowledgeIntelligence.Infrastructure;

/// <summary>
/// Shared helpers for storing collection-valued domain properties as JSONB columns:
/// the (de)serialization round-trips plus the EF Core value comparers that snapshot them.
/// </summary>
internal static class JsonValueConversions
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    public static List<T> DeserializeList<T>(string json) => JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];

    public static Dictionary<string, object?> DeserializeDictionary(string json) => JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions) ?? [];

    public static List<Citation> DeserializeCitations(string json) => JsonSerializer.Deserialize<List<Citation>>(json, JsonOptions) ?? [];

    public static string DeserializeString(string json) => JsonSerializer.Deserialize<string>(json, JsonOptions) ?? string.Empty;

    public static ValueComparer<List<T>> ListComparer<T>()
    {
        return new ValueComparer<List<T>>(
            (left, right) => JsonSerializer.Serialize(left, JsonOptions) == JsonSerializer.Serialize(right, JsonOptions),
            value => JsonSerializer.Serialize(value, JsonOptions).GetHashCode(StringComparison.Ordinal),
            value => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions) ?? new List<T>());
    }

    public static ValueComparer<Dictionary<string, object?>> DictionaryComparer()
    {
        return new ValueComparer<Dictionary<string, object?>>(
            (left, right) => JsonSerializer.Serialize(left, JsonOptions) == JsonSerializer.Serialize(right, JsonOptions),
            value => JsonSerializer.Serialize(value, JsonOptions).GetHashCode(StringComparison.Ordinal),
            value => JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions) ?? new Dictionary<string, object?>());
    }
}

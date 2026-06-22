using System.Text.Json;

namespace LocalKnowledgeIntelligence.Domain;

public static class MetadataExtensions
{
    public const int DefaultMaxSerializedBytes = 16 * 1024;

    public static Dictionary<string, object?> NormalizeMetadata(this Dictionary<string, object?>? metadata)
    {
        return metadata is null
            ? []
            : metadata
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
                .ToDictionary(pair => pair.Key.Trim(), pair => NormalizeValue(pair.Value), StringComparer.OrdinalIgnoreCase);
    }

    public static int GetSerializedByteCount(this Dictionary<string, object?>? metadata)
    {
        return JsonSerializer.SerializeToUtf8Bytes(metadata ?? []).Length;
    }

    private static object? NormalizeValue(object? value)
    {
        return value is JsonElement element
            ? element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
                JsonValueKind.Number when element.TryGetDouble(out var doubleValue) => doubleValue,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray().Select(item => NormalizeValue(item)).ToArray(),
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            }
            : value;
    }
}

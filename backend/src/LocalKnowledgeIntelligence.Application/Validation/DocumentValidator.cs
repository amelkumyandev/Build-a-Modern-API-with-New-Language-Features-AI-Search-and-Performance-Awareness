using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class DocumentValidator
{
    public void ValidateCreate(CreateDocumentRequest request) => Validate(request.Title, request.Content, request.Tags, request.Metadata, status: null);

    public DocumentStatus ValidateUpdate(UpdateDocumentRequest request)
    {
        if (!Enum.TryParse<DocumentStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw ValidationFailureException.For("status", "Status must be Draft, Created, Indexed, or Archived.");
        }

        Validate(request.Title, request.Content, request.Tags, request.Metadata, status);
        return status;
    }

    private static void Validate(string title, string content, IReadOnlyCollection<string>? tags, Dictionary<string, object?>? metadata, DocumentStatus? status)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        AddRangeError(errors, "title", title, 3, 200, "Title");
        AddRangeError(errors, "content", content, 50, 200_000, "Content");

        if (tags is { Count: > 20 })
        {
            Add(errors, "tags", "A document can have at most 20 tags.");
        }

        foreach (var tag in tags ?? [])
        {
            var trimmed = tag?.Trim() ?? string.Empty;
            if (trimmed.Length is < 2 or > 50)
            {
                Add(errors, "tags", "Each tag must be between 2 and 50 characters.");
                break;
            }
        }

        if (metadata.GetSerializedByteCount() > MetadataExtensions.DefaultMaxSerializedBytes)
        {
            Add(errors, "metadata", "Metadata cannot exceed 16 KB when serialized.");
        }

        if (status is DocumentStatus.Archived && string.IsNullOrWhiteSpace(content))
        {
            Add(errors, "status", "Archived documents must still preserve content.");
        }

        ThrowIfAny(errors);
    }

    public static SearchMode ParseSearchMode(string? raw, SearchMode defaultMode = SearchMode.Hybrid)
    {
        return string.IsNullOrWhiteSpace(raw)
            ? defaultMode
            : Enum.TryParse<SearchMode>(raw, ignoreCase: true, out var mode)
                ? mode
                : throw ValidationFailureException.For("searchMode", "Search mode must be Keyword, Semantic, or Hybrid.");
    }

    public static void ValidateQuery(string? query, int limit, int maxLimit)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query))
        {
            Add(errors, "query", "Query is required.");
        }
        else if (query.Trim().Length is < 2 or > 1000)
        {
            Add(errors, "query", "Query must be between 2 and 1000 characters.");
        }

        if (limit is < 1 || limit > maxLimit)
        {
            Add(errors, "limit", $"Limit must be between 1 and {maxLimit}.");
        }

        ThrowIfAny(errors);
    }

    public static void ValidateAgentMessage(AgentChatRequest request, int topK)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            Add(errors, "message", "Message is required.");
        }
        else if (request.Message.Trim().Length is < 2 or > 4000)
        {
            Add(errors, "message", "Message must be between 2 and 4000 characters.");
        }

        if (topK is < 1 or > 20)
        {
            Add(errors, "topK", "TopK must be between 1 and 20.");
        }

        _ = ParseSearchMode(request.SearchMode);
        ThrowIfAny(errors);
    }

    private static void AddRangeError(Dictionary<string, List<string>> errors, string key, string? value, int min, int max, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(errors, key, $"{label} is required.");
            return;
        }

        var length = value.Trim().Length;
        if (length < min || length > max)
        {
            Add(errors, key, $"{label} must be between {min} and {max} characters.");
        }
    }

    private static void Add(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }

    private static void ThrowIfAny(Dictionary<string, List<string>> errors)
    {
        if (errors.Count > 0)
        {
            throw new ValidationFailureException(errors.ToDictionary(pair => pair.Key, pair => pair.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase));
        }
    }
}

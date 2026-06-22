namespace LocalKnowledgeIntelligence.Contracts;

public sealed record UpdateDocumentRequest(
    string Title,
    string Content,
    IReadOnlyCollection<string>? Tags,
    Dictionary<string, object?>? Metadata,
    string Status);

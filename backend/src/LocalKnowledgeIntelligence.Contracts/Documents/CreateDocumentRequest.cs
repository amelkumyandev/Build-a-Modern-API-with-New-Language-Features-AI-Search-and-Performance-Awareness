namespace LocalKnowledgeIntelligence.Contracts;

public sealed record CreateDocumentRequest(
    string Title,
    string Content,
    IReadOnlyCollection<string>? Tags,
    Dictionary<string, object?>? Metadata);

namespace LocalKnowledgeIntelligence.Contracts;

public sealed record SearchResponse(string Query, IReadOnlyList<SearchResultResponse> Items);

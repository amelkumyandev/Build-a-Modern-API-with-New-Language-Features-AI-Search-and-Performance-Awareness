namespace LocalKnowledgeIntelligence.Application;

public sealed class AgentPromptBuilder
{
    public string Build(string message, IReadOnlyList<SearchHit> hits)
    {
        var context = hits.Count == 0
            ? "No local context was retrieved."
            : string.Join("\n\n", hits.Select((hit, index) =>
                $"Source {index + 1}\nDocumentId: {hit.DocumentId}\nChunkId: {hit.ChunkId}\nTitle: {hit.Title}\nScore: {hit.FinalScore:0.000}\nSnippet: {hit.Snippet}"));

        return $"""
        You are the Local Knowledge Intelligence agent.

        Use the retrieved local context to answer. If the context is insufficient, say so clearly.
        Do not invent citations. Do not claim the local database contains facts that are not in the context.

        User question:
        {message}

        Retrieved local context:
        {context}

        Return a concise expert answer grounded in the local context.
        """;
    }
}

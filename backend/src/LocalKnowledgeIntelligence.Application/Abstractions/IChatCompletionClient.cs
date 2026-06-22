namespace LocalKnowledgeIntelligence.Application;

public interface IChatCompletionClient
{
    Task<GeneratedAnswer> GenerateAnswerAsync(string prompt, CancellationToken cancellationToken);
}

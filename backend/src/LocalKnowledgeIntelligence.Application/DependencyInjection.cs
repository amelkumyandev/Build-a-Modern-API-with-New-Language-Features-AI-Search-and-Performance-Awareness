using Microsoft.Extensions.DependencyInjection;

namespace LocalKnowledgeIntelligence.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the application-layer use-case services. Options instances
    /// (<see cref="OpenAiOptions"/>, <see cref="ChunkingOptions"/>, <see cref="SearchOptions"/>)
    /// are bound from configuration in the composition root and must be registered there.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<RuntimeSettings>();
        services.AddSingleton<DocumentChunkingService>();
        services.AddSingleton<AgentPromptBuilder>();
        services.AddSingleton<EvaluationScorer>();
        services.AddScoped<DocumentValidator>();
        services.AddScoped<AuthService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<SearchService>();
        services.AddScoped<AgentOrchestrator>();
        services.AddScoped<SeedDataService>();
        services.AddScoped<EvaluationService>();
        return services;
    }
}

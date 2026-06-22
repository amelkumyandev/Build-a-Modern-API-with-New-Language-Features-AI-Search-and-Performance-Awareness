using LocalKnowledgeIntelligence.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LocalKnowledgeIntelligence.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=lki;Username=lki;Password=lki";

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddHttpClient<IEmbeddingClient, OpenAiEmbeddingClient>();
        services.AddHttpClient<IChatCompletionClient, OpenAiChatCompletionClient>();
        return services;
    }
}

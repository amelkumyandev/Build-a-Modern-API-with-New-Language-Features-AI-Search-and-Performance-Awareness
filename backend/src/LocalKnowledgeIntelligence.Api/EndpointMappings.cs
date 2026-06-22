using LocalKnowledgeIntelligence.Application;

namespace LocalKnowledgeIntelligence.Api;

public static class EndpointMappings
{
    public static IEndpointRouteBuilder MapLkiEndpoints(this IEndpointRouteBuilder endpoints, OpenAiOptions openAiOptions)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapAdminEndpoints(openAiOptions);
        endpoints.MapDocumentEndpoints();
        endpoints.MapSearchEndpoints();
        endpoints.MapAgentEndpoints();
        endpoints.MapEvaluationEndpoints();
        return endpoints;
    }
}

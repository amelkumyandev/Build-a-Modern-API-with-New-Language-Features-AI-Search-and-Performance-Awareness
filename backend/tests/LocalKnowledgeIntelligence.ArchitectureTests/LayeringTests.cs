using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.ArchitectureTests;

public sealed class LayeringTests
{
    [Fact]
    public void Domain_DoesNotReferenceInfrastructureOrAspNet()
    {
        var references = typeof(KnowledgeDocument).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();

        Assert.DoesNotContain("LocalKnowledgeIntelligence.Infrastructure", references);
        Assert.DoesNotContain(references, name => name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Contracts_DoNotReferenceInfrastructure()
    {
        var references = typeof(LoginRequest).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();

        Assert.DoesNotContain("LocalKnowledgeIntelligence.Infrastructure", references);
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructure()
    {
        var references = typeof(DocumentService).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();

        Assert.DoesNotContain("LocalKnowledgeIntelligence.Infrastructure", references);
    }
}

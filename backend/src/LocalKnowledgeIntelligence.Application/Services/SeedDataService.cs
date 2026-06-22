using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class SeedDataService(
    IDocumentRepository documents,
    IDocumentChunkRepository chunks,
    IUserRepository users,
    IEvaluationRepository evaluations,
    IPasswordHasher passwordHasher,
    IClock clock)
{
    public async Task EnsureAdminAsync(CancellationToken cancellationToken)
    {
        if (await users.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = User.CreateAdmin("admin", passwordHasher.HashPassword("admin"), clock.UtcNow);
        await users.AddAsync(admin, cancellationToken);
    }

    public async Task<SeedDocumentsResponse> SeedDemoDataAsync(CancellationToken cancellationToken)
    {
        var admin = await users.GetByUsernameAsync("admin", cancellationToken)
            ?? throw new NotFoundException("The admin user was not found.");

        var existingByTitle = (await documents.GetAllActiveAsync(cancellationToken))
            .GroupBy(document => document.Title, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var created = 0;
        foreach (var seed in SeedCatalog.Documents())
        {
            if (existingByTitle.TryGetValue(seed.Title, out var existing))
            {
                if (IsGeneratedSeed(existing) && existing.Content != seed.Content)
                {
                    existing.Update(seed.Title, seed.Content, seed.Tags, seed.Metadata, DocumentStatus.Created, clock.UtcNow);
                    existing.SetSummary(seed.Summary, clock.UtcNow);
                }

                continue;
            }

            var document = KnowledgeDocument.Create(seed.Title, seed.Content, seed.Tags, seed.Metadata, admin.Id, clock.UtcNow);
            document.SetSummary(seed.Summary, clock.UtcNow);
            await documents.AddAsync(document, cancellationToken);
            created++;
        }

        var questionsCreated = await evaluations.AddQuestionsIfMissingAsync(SeedCatalog.Questions(clock.UtcNow), cancellationToken);
        await documents.SaveChangesAsync(cancellationToken);
        return new SeedDocumentsResponse(created, questionsCreated);
    }

    private static bool IsGeneratedSeed(KnowledgeDocument document)
    {
        return document.Metadata.TryGetValue("source", out var source)
            && string.Equals(source?.ToString(), "generated", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync(bool databaseReady, bool openAiKeyConfigured, CancellationToken cancellationToken)
    {
        var latest = await evaluations.GetLatestRunAsync(cancellationToken);
        return new AdminDashboardResponse(
            databaseReady,
            openAiKeyConfigured,
            await documents.CountActiveAsync(cancellationToken),
            await chunks.CountAsync(cancellationToken),
            await chunks.CountEmbeddedAsync(cancellationToken),
            0,
            latest?.Score,
            "Development Mode");
    }
}

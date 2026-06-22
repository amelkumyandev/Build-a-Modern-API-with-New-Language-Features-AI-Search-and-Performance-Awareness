using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;
using LocalKnowledgeIntelligence.Infrastructure;

namespace LocalKnowledgeIntelligence.UnitTests;

public sealed class CoreBehaviorTests
{
    [Fact]
    public void TagNormalization_TrimsLowercasesSortsAndDeduplicates()
    {
        var tags = new[] { "  RAG ", "dotnet", "rag", "", " PGVector " }.NormalizeTags();

        Assert.Equal(["dotnet", "pgvector", "rag"], tags);
    }

    [Fact]
    public void DocumentValidator_RejectsInvalidCreateRequest()
    {
        var validator = new DocumentValidator();

        var ex = Assert.Throws<ValidationFailureException>(() =>
            validator.ValidateCreate(new CreateDocumentRequest("x", "short", Enumerable.Range(0, 25).Select(i => $"tag{i}").ToArray(), [])));

        Assert.Contains("title", ex.Errors.Keys);
        Assert.Contains("content", ex.Errors.Keys);
        Assert.Contains("tags", ex.Errors.Keys);
    }

    [Fact]
    public void ValidationFailure_ForBuildsSingleFieldErrors()
    {
        var ex = ValidationFailureException.For("query", "Query is required.", "Query must be specific.");

        Assert.True(ex.Errors.ContainsKey("query"));
        Assert.Equal(["Query is required.", "Query must be specific."], ex.Errors["query"]);
    }

    [Fact]
    public void Options_NormalizeUnsafeOrBlankValues()
    {
        var jwt = new JwtOptions
        {
            Issuer = "  ",
            Audience = "  Admins  ",
            ExpirationHours = 0,
            SigningKey = "  "
        };
        var openAi = new OpenAiOptions
        {
            ApiKey = "  key  ",
            EmbeddingModel = "  ",
            ChatModel = "  gpt-test  ",
            EmbeddingDimensions = 0
        };
        var chunking = new ChunkingOptions { TargetTokenCount = 0, OverlapTokenCount = -1 };
        var search = new SearchOptions { DefaultLimit = 0, MaxLimit = -1, VectorWeight = 2, KeywordWeight = -1, RecencyWeight = 0.5 };

        Assert.Equal("LocalKnowledgeIntelligence", jwt.Issuer);
        Assert.Equal("Admins", jwt.Audience);
        Assert.Equal(1, jwt.ExpirationHours);
        Assert.Equal("local-dev-signing-key-change-me", jwt.SigningKey);
        Assert.Equal("key", openAi.ApiKey);
        Assert.Equal("text-embedding-3-small", openAi.EmbeddingModel);
        Assert.Equal("gpt-test", openAi.ChatModel);
        Assert.Equal(1, openAi.EmbeddingDimensions);
        Assert.Equal(1, chunking.TargetTokenCount);
        Assert.Equal(0, chunking.OverlapTokenCount);
        Assert.Equal(1, search.DefaultLimit);
        Assert.Equal(1, search.MaxLimit);
        Assert.Equal(1, search.VectorWeight);
        Assert.Equal(0, search.KeywordWeight);
        Assert.Equal(0.5, search.RecencyWeight);
    }

    [Fact]
    public void Chunking_CreatesDeterministicChunks()
    {
        var document = KnowledgeDocument.Create(
            "Hybrid Search Architecture",
            string.Join("\n\n", Enumerable.Range(1, 20).Select(i => $"Paragraph {i} explains local vector search, keyword retrieval, and citations in a .NET RAG platform.")),
            ["search"],
            [],
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        var chunker = new DocumentChunkingService(new ChunkingOptions { TargetTokenCount = 80, OverlapTokenCount = 20 });
        var chunks = chunker.Chunk(document);

        Assert.True(chunks.Count > 1);
        Assert.Equal(0, chunks[0].Index);
        Assert.All(chunks, chunk => Assert.True(chunk.TokenEstimate > 0));
    }

    [Fact]
    public void HybridScoreCalculator_CombinesScoresWithConfiguredWeights()
    {
        var calculator = new HybridScoreCalculator(new SearchOptions { VectorWeight = 0.65, KeywordWeight = 0.30, RecencyWeight = 0.05 });

        var score = calculator.Calculate(0.8, 0.5, 1.0);

        Assert.InRange(score, 0.71, 0.73);
    }

    [Fact]
    public void AgentPromptBuilder_IncludesRetrievedSources()
    {
        var hit = new SearchHit(Guid.NewGuid(), Guid.NewGuid(), "Vector Search", "pgvector stores local embeddings.", 0.9, 0, 0, 0.9, 0.1, ["vector"], DateTimeOffset.UtcNow);

        var prompt = new AgentPromptBuilder().Build("How does vector search work?", [hit]);

        Assert.Contains("Vector Search", prompt);
        Assert.Contains("Do not invent citations", prompt);
    }

    [Fact]
    public void PasswordHasher_HashesAndVerifiesAdminPassword()
    {
        var hasher = new PasswordHasher();

        var hash = hasher.HashPassword("admin");

        Assert.NotEqual("admin", hash);
        Assert.True(hasher.Verify("admin", hash));
        Assert.False(hasher.Verify("wrong", hash));
    }

    [Fact]
    public void EvaluationScorer_ScoresExpectedDocumentMatches()
    {
        var question = EvaluationQuestion.Create("What is pgvector?", ["vector"], ["pgvector HNSW Index Operations"], "medium", DateTimeOffset.UtcNow);
        var hit = new SearchHit(Guid.NewGuid(), Guid.NewGuid(), "pgvector HNSW Index Operations", "HNSW index details", 0.9, 0, 0, 0.9, 0.1, ["vector"], DateTimeOffset.UtcNow);

        var score = new EvaluationScorer().Score(question, [hit]);

        Assert.Equal(1, score);
    }

    [Fact]
    public void SeedCatalog_DefaultChunkingProducesAtLeastSeventyFiveChunks()
    {
        var chunker = new DocumentChunkingService(new ChunkingOptions());
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var chunkCount = SeedCatalog.Documents()
            .Select(seed => KnowledgeDocument.Create(seed.Title, seed.Content, seed.Tags, seed.Metadata, userId, now))
            .Sum(document => chunker.Chunk(document).Count);

        Assert.True(chunkCount >= 75, $"Expected at least 75 chunks, but generated {chunkCount}.");
    }
}

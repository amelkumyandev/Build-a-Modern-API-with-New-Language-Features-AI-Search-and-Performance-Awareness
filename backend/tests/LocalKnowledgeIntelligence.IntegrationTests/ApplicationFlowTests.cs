using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;
using LocalKnowledgeIntelligence.Domain;
using LocalKnowledgeIntelligence.Infrastructure;

namespace LocalKnowledgeIntelligence.IntegrationTests;

public sealed class ApplicationFlowTests
{
    [Fact]
    public async Task AuthService_Login_WithSeededAdminSucceeds()
    {
        var clock = new FakeClock();
        var hasher = new PasswordHasher();
        var users = new FakeUsers();
        await users.AddAsync(User.CreateAdmin("admin", hasher.HashPassword("admin"), clock.UtcNow), CancellationToken.None);
        var auth = new AuthService(users, hasher, new FakeTokenService(), clock);

        var response = await auth.LoginAsync(new LoginRequest("admin", "admin"), CancellationToken.None);

        Assert.Equal("admin", response.User.Username);
        Assert.Equal("token", response.AccessToken);
    }

    [Fact]
    public async Task DocumentService_CreateChunkAndEmbedStoresVectorState()
    {
        var harness = new Harness();
        var create = new CreateDocumentRequest(
            "Hybrid Search Architecture for Local RAG",
            string.Join(" ", Enumerable.Repeat("Hybrid search combines pgvector semantic search with keyword retrieval and citations.", 20)),
            ["RAG", "pgvector"],
            new Dictionary<string, object?> { ["source"] = "test" });

        var created = await harness.Documents.CreateAsync(create, Guid.NewGuid(), CancellationToken.None);
        var chunked = await harness.Documents.ChunkAsync(created.Id, CancellationToken.None);
        var embedded = await harness.Documents.EmbedAsync(created.Id, CancellationToken.None);

        Assert.True(chunked.ChunksCreated > 0);
        Assert.Equal(chunked.ChunksCreated, embedded);
        Assert.Equal(embedded, harness.Search.StoredEmbeddings.Count);
    }

    [Fact]
    public async Task DocumentService_CreateAndIndexStoresEmbeddingsImmediately()
    {
        var harness = new Harness();
        var create = new CreateDocumentRequest(
            "Create And Embed Workflow for Local RAG",
            string.Join("\n\n", Enumerable.Range(1, 14).Select(i => $"Section {i} explains how save and embed creates chunks, generates embeddings, stores vectors, and returns indexed document status for local knowledge workflows.")),
            ["rag", "embeddings"],
            new Dictionary<string, object?> { ["source"] = "test" });

        var created = await harness.Documents.CreateAndIndexAsync(create, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("Indexed", created.Status);
        Assert.Equal("Chunked", created.ChunkingStatus);
        Assert.Equal("Embedded", created.EmbeddingStatus);
        Assert.True(harness.ChunkRepo.Items.Count > 0);
        Assert.Equal(harness.ChunkRepo.Items.Count, harness.Search.StoredEmbeddings.Count);
    }

    [Fact]
    public async Task SearchService_HybridReturnsScoreBreakdown()
    {
        var harness = new Harness();
        harness.Search.SemanticHits.Add(new SearchHit(Guid.NewGuid(), Guid.NewGuid(), "Vector Search", "Semantic pgvector hit", 0.9, 0, 0, 0.9, 0.1, ["vector"], harness.Clock.UtcNow));
        harness.Search.KeywordHits.Add(harness.Search.SemanticHits[0] with { KeywordScore = 5, MatchedFields = ["title"] });

        var result = await harness.SearchService.HybridAsync("vector search", 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.True(result.Items[0].FinalScore > 0);
        Assert.Contains("title", result.Items[0].MatchedFields);
    }

    [Fact]
    public async Task AgentOrchestrator_ReturnsAnswerWithCitations()
    {
        var harness = new Harness();
        var hit = new SearchHit(Guid.NewGuid(), Guid.NewGuid(), "Agent Citation Policy", "Answers must cite retrieved local chunks.", 0.9, 0.4, 0.1, 0.8, 0.1, ["vector"], harness.Clock.UtcNow);
        harness.Search.SemanticHits.Add(hit);
        harness.Search.KeywordHits.Add(hit);

        var response = await harness.Agent.ChatAsync(Guid.NewGuid(), new AgentChatRequest(null, "How should citations work?", "Hybrid", 8), CancellationToken.None);

        Assert.Contains("grounded answer", response.Answer);
        Assert.Single(response.Citations);
    }

    [Fact]
    public async Task EvaluationService_RunStoresScore()
    {
        var harness = new Harness();
        var question = EvaluationQuestion.Create("What is pgvector?", ["vector"], ["pgvector HNSW Index Operations"], "medium", harness.Clock.UtcNow);
        await harness.Evaluations.AddQuestionsIfMissingAsync([question], CancellationToken.None);
        harness.Search.SemanticHits.Add(new SearchHit(Guid.NewGuid(), Guid.NewGuid(), "pgvector HNSW Index Operations", "HNSW index", 0.9, 0, 0, 0.9, 0.1, ["vector"], harness.Clock.UtcNow));

        var run = await harness.Evaluation.RunAsync(new EvaluationRunRequest("Semantic", 5), CancellationToken.None);

        Assert.Equal(1, run.QuestionCount);
        Assert.Equal(1, run.Score);
        var result = Assert.Single(run.Results);
        Assert.True(result.Passed);
        Assert.Equal(question.Id, result.QuestionId);
        Assert.Contains("pgvector HNSW Index Operations", result.MatchedExpectedDocumentTitles);
        Assert.Contains("vector", result.MatchedExpectedKeywords);
        Assert.Single(result.RetrievedChunks);
    }

    private sealed class Harness
    {
        public FakeClock Clock { get; } = new();
        public FakeDocuments DocumentRepo { get; } = new();
        public FakeChunks ChunkRepo { get; } = new();
        public FakeSearch Search { get; } = new();
        public FakeEvaluations Evaluations { get; } = new();
        public SearchService SearchService { get; }
        public DocumentService Documents { get; }
        public AgentOrchestrator Agent { get; }
        public EvaluationService Evaluation { get; }

        public Harness()
        {
            var openAi = new OpenAiOptions { ApiKey = "test", EmbeddingModel = "test-embedding", ChatModel = "test-chat", EmbeddingDimensions = 3 };
            var searchOptions = new SearchOptions();
            var settings = new RuntimeSettings(openAi, new ChunkingOptions(), searchOptions);
            SearchService = new SearchService(Search, new FakeEmbeddingClient(), settings, Clock);
            Documents = new DocumentService(DocumentRepo, ChunkRepo, Search, new FakeEmbeddingClient(), new DocumentValidator(), new DocumentChunkingService(new ChunkingOptions { TargetTokenCount = 120, OverlapTokenCount = 20 }), openAi, searchOptions, Clock);
            Agent = new AgentOrchestrator(new FakeAgentRepository(), SearchService, new FakeChatClient(), new AgentPromptBuilder(), settings, Clock);
            Evaluation = new EvaluationService(Evaluations, SearchService, new EvaluationScorer(), Clock);
        }
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 6, 19, 8, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeTokenService : ITokenService
    {
        public LoginResponse CreateToken(User user, DateTimeOffset now) => new("token", now.AddHours(8), user.ToResponse());
    }

    private sealed class FakeEmbeddingClient : IEmbeddingClient
    {
        public Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken) => Task.FromResult(new[] { 0.1f, 0.2f, 0.3f });
    }

    private sealed class FakeChatClient : IChatCompletionClient
    {
        public Task<GeneratedAnswer> GenerateAnswerAsync(string prompt, CancellationToken cancellationToken) => Task.FromResult(new GeneratedAnswer("This is a grounded answer with citations."));
    }

    private sealed class FakeUsers : IUserRepository
    {
        private readonly List<User> _users = [];
        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken) => Task.FromResult(_users.FirstOrDefault(user => user.Username == username));
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_users.FirstOrDefault(user => user.Id == id));
        public Task<bool> AnyAsync(CancellationToken cancellationToken) => Task.FromResult(_users.Count > 0);
        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDocuments : IDocumentRepository
    {
        public List<KnowledgeDocument> Items { get; } = [];
        public Task AddAsync(KnowledgeDocument document, CancellationToken cancellationToken)
        {
            Items.Add(document);
            return Task.CompletedTask;
        }
        public Task<KnowledgeDocument?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(document => document.Id == id && (includeDeleted || !document.IsDeleted)));
        public Task<PagedResult<KnowledgeDocument>> ListAsync(int pageNumber, int pageSize, IReadOnlyList<string> tags, CancellationToken cancellationToken) => Task.FromResult(new PagedResult<KnowledgeDocument>(Items, pageNumber, pageSize, Items.Count));
        public Task<IReadOnlyList<KnowledgeDocument>> GetAllActiveAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<KnowledgeDocument>>(Items.Where(document => !document.IsDeleted).ToArray());
        public Task<int> CountActiveAsync(CancellationToken cancellationToken) => Task.FromResult(Items.Count(document => !document.IsDeleted));
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeChunks : IDocumentChunkRepository
    {
        public List<DocumentChunk> Items { get; } = [];
        public Task ReplaceForDocumentAsync(Guid documentId, IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken)
        {
            Items.RemoveAll(chunk => chunk.DocumentId == documentId);
            Items.AddRange(chunks);
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DocumentChunk>>(Items.Where(chunk => chunk.DocumentId == documentId).ToArray());
        public Task<IReadOnlyList<DocumentChunk>> GetChunksMissingEmbeddingsAsync(Guid documentId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<DocumentChunk>>(Items.Where(chunk => chunk.DocumentId == documentId && chunk.EmbeddingGeneratedAt is null).ToArray());
        public Task<DocumentChunk?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(chunk => chunk.Id == id));
        public Task<int> CountAsync(CancellationToken cancellationToken) => Task.FromResult(Items.Count);
        public Task<int> CountEmbeddedAsync(CancellationToken cancellationToken) => Task.FromResult(Items.Count(chunk => chunk.EmbeddingGeneratedAt is not null));
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeSearch : ISearchRepository
    {
        public List<SearchHit> KeywordHits { get; } = [];
        public List<SearchHit> SemanticHits { get; } = [];
        public List<Guid> StoredEmbeddings { get; } = [];
        public Task<IReadOnlyList<SearchHit>> KeywordSearchAsync(string query, int limit, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<SearchHit>>(KeywordHits.Take(limit).ToArray());
        public Task<IReadOnlyList<SearchHit>> SemanticSearchAsync(float[] embedding, int limit, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<SearchHit>>(SemanticHits.Take(limit).ToArray());
        public Task StoreEmbeddingAsync(Guid chunkId, float[] embedding, string model, int dimensions, DateTimeOffset generatedAt, CancellationToken cancellationToken)
        {
            StoredEmbeddings.Add(chunkId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAgentRepository : IAgentRepository
    {
        private readonly List<ChatSession> _sessions = [];
        private readonly List<ChatMessage> _messages = [];
        private readonly List<AgentRun> _runs = [];

        public Task<ChatSession?> GetSessionAsync(Guid id, Guid userId, CancellationToken cancellationToken) => Task.FromResult(_sessions.FirstOrDefault(session => session.Id == id && session.UserId == userId));
        public Task<IReadOnlyList<ChatSession>> ListSessionsAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ChatSession>>(_sessions.Where(session => session.UserId == userId).ToArray());
        public Task AddSessionAsync(ChatSession session, CancellationToken cancellationToken) { _sessions.Add(session); return Task.CompletedTask; }
        public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken) { _messages.Add(message); return Task.CompletedTask; }
        public Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ChatMessage>>(_messages.Where(message => message.SessionId == sessionId).ToArray());
        public Task AddRunAsync(AgentRun run, CancellationToken cancellationToken) { _runs.Add(run); return Task.CompletedTask; }
        public Task<AgentRun?> GetRunAsync(Guid runId, CancellationToken cancellationToken) => Task.FromResult(_runs.FirstOrDefault(run => run.Id == runId));
        public Task AddStepAsync(AgentStep step, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeEvaluations : IEvaluationRepository
    {
        private readonly List<EvaluationQuestion> _questions = [];
        private readonly List<EvaluationRun> _runs = [];
        public Task<int> AddQuestionsIfMissingAsync(IReadOnlyList<EvaluationQuestion> questions, CancellationToken cancellationToken)
        {
            var created = questions.Where(question => _questions.All(existing => existing.Question != question.Question)).ToArray();
            _questions.AddRange(created);
            return Task.FromResult(created.Length);
        }
        public Task<IReadOnlyList<EvaluationQuestion>> ListQuestionsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<EvaluationQuestion>>(_questions);
        public Task AddRunAsync(EvaluationRun run, CancellationToken cancellationToken) { _runs.Add(run); return Task.CompletedTask; }
        public Task<EvaluationRun?> GetRunAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_runs.FirstOrDefault(run => run.Id == id));
        public Task<EvaluationRun?> GetLatestRunAsync(CancellationToken cancellationToken) => Task.FromResult(_runs.LastOrDefault());
    }
}

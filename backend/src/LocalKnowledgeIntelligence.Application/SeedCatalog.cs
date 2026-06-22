using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed record SeedDocument(string Title, string Summary, string Content, IReadOnlyList<string> Tags, Dictionary<string, object?> Metadata);

public static class SeedCatalog
{
    private static readonly string[] Topics =
    [
        "NET 10 API Runtime Modernization",
        "C# 14 Extension Member Guidelines",
        "ASP.NET Core Minimal API Endpoint Design",
        "EF Core PostgreSQL Migration Strategy",
        "pgvector HNSW Index Operations",
        "OpenAI Embedding Pipeline",
        "Hybrid Search Scoring Model",
        "Retrieval Augmented Generation Guardrails",
        "Agent Citation Policy",
        "JWT Authentication for Local Admin Tools",
        "Docker Compose Local Development",
        "Next.js Admin Dashboard Architecture",
        "Document Chunking and Token Estimation",
        "Semantic Search Failure Modes",
        "Evaluation Dataset Design",
        "Structured Logging for Retrieval Systems",
        "ProblemDetails Error Contracts",
        "Password Hashing and Secret Handling",
        "PostgreSQL JSONB Metadata Design",
        "Vector Search Performance Tuning",
        "OpenAI Configuration and Key Safety",
        "Integration Testing with Fake AI Providers",
        "Clean Architecture Boundaries",
        "Agent Run Inspection",
        "Local Knowledge Platform Threat Model"
    ];

    public static IReadOnlyList<SeedDocument> Documents()
    {
        return Topics.Select((topic, index) =>
        {
            var tags = TagsFor(topic);
            var content = BuildContent(topic, tags);

            return new SeedDocument(
                topic,
                $"Practical guidance for {topic.ToLowerInvariant()} in a local .NET RAG platform.",
                content,
                tags,
                new Dictionary<string, object?>
                {
                    ["difficulty"] = index % 3 == 0 ? "advanced" : index % 3 == 1 ? "intermediate" : "expert",
                    ["source"] = "generated",
                    ["ordinal"] = index + 1
                });
        }).ToArray();
    }

    public static IReadOnlyList<EvaluationQuestion> Questions(DateTimeOffset now)
    {
        var questions = new List<EvaluationQuestion>();

        foreach (var topic in Topics)
        {
            questions.Add(EvaluationQuestion.Create(
                $"How should the platform handle {topic.ToLowerInvariant()}?",
                ["local", "retrieval", "citations"],
                [topic],
                "medium",
                now));

            questions.Add(EvaluationQuestion.Create(
                $"What implementation risks should be considered for {topic.ToLowerInvariant()}?",
                ["validation", "observability", "security"],
                [topic],
                "advanced",
                now));
        }

        return questions;
    }

    private static string BuildContent(string topic, IReadOnlyList<string> tags)
    {
        var tagList = string.Join(", ", tags);
        var subject = topic.ToLowerInvariant();
        var sections = new[]
        {
            ("Overview", $"This generated technical document is part of the Local Knowledge Intelligence training corpus. It explains {subject} for a local-first RAG system built with .NET 10, C# 14, ASP.NET Core, EF Core, PostgreSQL, pgvector, OpenAI, and Next.js. The goal is to give the agent enough precise local context to answer implementation questions with citations instead of relying on unstated external assumptions."),
            ("Architecture Role", $"In the platform architecture, {subject} belongs inside a clean boundary. Domain rules stay independent from database, HTTP, and OpenAI details. Application services coordinate use cases, infrastructure owns persistence and external clients, and the API layer remains thin. This separation keeps retrieval behavior testable and lets the frontend inspect document, chunk, embedding, search, and agent state without leaking infrastructure concerns."),
            ("Data Model Impact", $"The database records for {subject} must support repeatable local development. Documents keep title, content, summary, normalized tags, JSONB metadata, status, timestamps, and a soft-delete marker. Chunks keep source document ID, index, token estimate, metadata, embedding model, embedding dimensions, generation timestamp, and a pgvector embedding. Tags for this seed topic are {tagList}, which helps keyword and hybrid retrieval exercise multiple matched fields."),
            ("Ingestion Flow", $"A safe ingestion flow validates title, content, tags, and metadata before persistence. After a document is saved, deterministic chunking splits content on paragraph boundaries where practical and stores stable chunk indexes. Re-indexing replaces old chunks, invalidates old embeddings, and generates fresh vectors. This makes {subject} useful for demonstrations because each run produces predictable chunks and avoids duplicate seed documents."),
            ("Embedding Flow", $"Embeddings for {subject} are generated from chunk text plus document context such as title, tags, and chunk index. The backend sends only selected chunk-level content to OpenAI, never the whole corpus. The API key is read from environment configuration, is never committed, and is not logged. Stored vectors use the configured embedding model and dimensions so future model migrations can identify stale embeddings."),
            ("Search Behavior", $"Keyword search is valuable for exact identifiers, framework names, table names, method names, and operational phrases in {subject}. Semantic search is valuable when the user asks with different wording but similar intent. Hybrid search combines vector similarity, normalized keyword score, and a small recency boost. Result objects include document ID, chunk ID, title, snippet, vector score, keyword score, recency score, final score, distance, and matched fields."),
            ("Agent Behavior", $"The agent must retrieve local chunks before answering questions about {subject}. It should use hybrid search by default, cite retrieved chunks for important factual claims, and state when the local knowledge base is insufficient. It must not invent citations, expose secrets, execute shell commands, run arbitrary SQL, or modify documents outside the explicit document APIs. Every run stores retrieval, answer generation, and citation steps for later inspection."),
            ("Validation Rules", $"Validation for {subject} keeps the demo realistic and production-shaped. Document titles are required and bounded, content must be substantial, tags are normalized and limited, metadata has a serialized size limit, search queries are bounded, and agent messages have a top-k cap. These checks fail with ProblemDetails-style responses so the frontend can show useful errors without exposing stack traces or infrastructure internals."),
            ("Performance Considerations", $"Performance work for {subject} starts with bounded requests. Document lists are paginated, search limits are capped, read queries use no-tracking EF Core patterns where possible, vector search excludes chunks without embeddings, and agent prompts include only the top retrieved chunks. PostgreSQL indexes include HNSW for vectors and trigram support for chunk text so local search remains responsive as the seed corpus grows."),
            ("Observability", $"Operational visibility for {subject} includes structured logs, health checks, dashboard counters, and persisted agent run steps. Useful signals include database readiness, OpenAI key configuration, total documents, total chunks, embedded chunks, failed embeddings, latest evaluation score, retrieval duration, answer generation duration, number of retrieved chunks, and citation count. Logs should carry context without recording full prompts or secrets."),
            ("Testing Strategy", $"Tests for {subject} should keep external dependencies replaceable. Unit tests cover validators, tag normalization, chunking, scoring, prompt construction, password hashing, and evaluation scoring. Integration-flow tests use fake embedding and chat providers to verify document creation, chunking, embedding state, hybrid scoring, agent citations, and evaluation persistence. Architecture tests protect dependency direction between domain, application, contracts, infrastructure, and API layers."),
            ("Local Operations", $"During local development, {subject} runs through Docker Compose with PostgreSQL and pgvector. The seed action creates realistic technical documents and evaluation questions without duplicates, and re-indexing creates embeddings with the configured OpenAI model. The default admin/admin account and local JWT key are demo-only settings; they are visible warnings in development and must be changed before any non-local deployment."),
            ("Failure Handling", $"Failure handling for {subject} should be predictable. Validation problems return field-level ProblemDetails, missing OpenAI configuration returns a service-unavailable response with a setup hint, and unexpected exceptions are logged with request context. The application should preserve enough document state for an admin to retry chunking or embedding without recreating the source material."),
            ("Security Review", $"A security review for {subject} checks authentication, authorization, prompt boundaries, logging, and data access. JWT protects admin, document, search, agent, and evaluation APIs. EF Core and parameterized database commands avoid injection risk. The agent receives retrieved context, not unrestricted database access, and the frontend never receives secrets or backend stack traces."),
            ("Frontend Workflow", $"The frontend workflow for {subject} gives admins a direct path from document creation to retrieval validation. Admins can save a document, save and embed it immediately, inspect chunks, run keyword or semantic or hybrid search, open the source document, ask the agent about a retrieved result, inspect citations, and review agent run steps. Loading and error states keep local operations understandable."),
            ("Evaluation Signals", $"Evaluation signals for {subject} help decide whether retrieval is improving. Seed questions name expected source documents and keywords. A run performs retrieval for each question, scores whether expected material appeared in the top results, stores the score, and updates the dashboard. This is intentionally simple for version one, but it creates a stable baseline for future answer faithfulness and reranking experiments."),
            ("Deployment Notes", $"Deployment notes for {subject} focus on keeping local assumptions visible. Docker Compose is appropriate for development, while any shared environment must rotate the admin password, replace the JWT signing key, secure PostgreSQL credentials, configure CORS deliberately, and decide how migrations are applied. The same clean application services can later run behind stronger identity and observability infrastructure."),
            ("Extension Path", $"The extension path for {subject} is deliberately incremental. Streaming answers, file upload for PDF and DOCX, background embedding jobs, reranking, OpenTelemetry traces, per-document access control, multiple workspaces, Azure OpenAI, local embedding models, and richer evaluation reports can be added after the core local vector database, retrieval, citation, and dashboard workflows are stable.")
        };

        return string.Join("\n\n", sections.Select(section => $"{topic} - {section.Item1}\n\n{section.Item2}"));
    }

    private static IReadOnlyList<string> TagsFor(string topic)
    {
        var lower = topic.ToLowerInvariant();
        var tags = new List<string> { "local-rag", "dotnet" };

        if (lower.Contains("openai") || lower.Contains("embedding"))
        {
            tags.Add("openai");
            tags.Add("embeddings");
        }

        if (lower.Contains("vector") || lower.Contains("pgvector") || lower.Contains("semantic"))
        {
            tags.Add("pgvector");
            tags.Add("semantic-search");
        }

        if (lower.Contains("agent") || lower.Contains("citation") || lower.Contains("rag"))
        {
            tags.Add("agent");
            tags.Add("citations");
        }

        if (lower.Contains("next"))
        {
            tags.Add("nextjs");
        }

        if (lower.Contains("docker") || lower.Contains("postgresql"))
        {
            tags.Add("postgresql");
            tags.Add("docker");
        }

        if (lower.Contains("auth") || lower.Contains("jwt") || lower.Contains("password"))
        {
            tags.Add("security");
        }

        return tags.NormalizeTags();
    }
}

# Local Knowledge Intelligence

Expert-level local RAG platform using .NET 10, C# 14, ASP.NET Core, EF Core, PostgreSQL/pgvector, OpenAI, JWT authentication, and a Next.js admin frontend.

## Local Setup

```bash
cp .env.example .env
```

Set these values in `.env`:

```bash
OPENAI_API_KEY=your_key_here
JWT_SIGNING_KEY=change_this_local_dev_key
```

Run the full stack:

```bash
docker compose up --build
```

Frontend: http://localhost:3000  
Backend: http://localhost:5000  
Swagger: http://localhost:5000/swagger  
Login: `admin` / `admin`

The default `admin/admin` account is for local development only. Change it before any real deployment.

## Documentation

- [Setup Guide](docs/setup.md)
- [Usage Guide](docs/usage.md)
- [.NET 9/10 and C# 13/14 Feature Usage](docs/dotnet10-csharp14-features.md)
- [Architecture](docs/architecture.md)
- [API](docs/api.md)
- [Vector Search](docs/vector-search.md)
- [Agent Design](docs/agent-design.md)
- [Testing](docs/testing.md)

## Backend Commands

```bash
dotnet restore backend/LocalKnowledgeIntelligence.sln
dotnet build backend/LocalKnowledgeIntelligence.sln -m:1
dotnet test backend/LocalKnowledgeIntelligence.sln --no-build -m:1
dotnet run --project backend/src/LocalKnowledgeIntelligence.Api
```

The `-m:1` flag avoids a .NET SDK/MSBuild diagnostic issue observed on this machine where parallel project-reference builds can fail silently.

## Frontend Commands

```bash
cd frontend
npm install
npm run build
npm run dev
```

With the Docker stack running, run the frontend/API smoke check:

```bash
cd frontend
npm run test:smoke
```

## Implemented Capabilities

- JWT login with seeded local admin user.
- Document create, list, detail, update, delete, chunk, embed, and re-index APIs.
- Optional create-and-index flow through `POST /api/documents?index=true`.
- PostgreSQL schema with `vector(1536)`, `pg_trgm`, JSONB metadata, HNSW vector index, and trigram index.
- OpenAI embedding client using `OPENAI_API_KEY`.
- OpenAI Responses API chat client for grounded agent answers.
- Keyword, semantic, and hybrid search endpoints.
- Agent chat with citations and stored run steps.
- Evaluation question generation, retrieval scoring, per-question results, and failed-case drilldown.
- Next.js admin UI for dashboard, documents, search, agent, evaluation, settings, create-and-embed, filters, citations, and agent run steps.
- Unit, integration-flow, and architecture tests.

## Learning Objectives

This project demonstrates production-shaped .NET backend design for AI systems: clean layering, .NET 10 targeting with C# 14 enabled, modern C# records and collection expressions, async EF Core access, JWT authentication, ProblemDetails error responses, OpenAI integration behind interfaces, deterministic chunking, vector search with pgvector, hybrid ranking, and frontend/backend integration through typed REST contracts.

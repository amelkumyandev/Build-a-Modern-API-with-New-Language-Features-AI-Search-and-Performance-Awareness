# Architecture

The solution uses a layered modular monolith:

- `LocalKnowledgeIntelligence.Domain` contains entities, enums, value-like records, and invariant behavior.
- `LocalKnowledgeIntelligence.Contracts` contains request and response contracts shared conceptually with the frontend.
- `LocalKnowledgeIntelligence.Application` contains use cases, validation, orchestration, search scoring, chunking, and service interfaces.
- `LocalKnowledgeIntelligence.Infrastructure` contains EF Core, PostgreSQL/pgvector SQL, OpenAI clients, JWT issuing, password hashing, repositories, migrations, and seed bootstrapping.
- `LocalKnowledgeIntelligence.Api` composes the HTTP API, JWT validation, Swagger, health checks, CORS, and ProblemDetails middleware.
- `frontend` contains the Next.js admin dashboard.

Domain and contracts do not reference infrastructure. Application depends on abstractions and is tested with fakes.

# Testing

Backend test projects:

- `LocalKnowledgeIntelligence.UnitTests` covers tag normalization, validation, chunking, hybrid scoring, prompt construction, password hashing, and evaluation scoring.
- `LocalKnowledgeIntelligence.IntegrationTests` covers application flows using fake repositories and fake AI providers.
- `LocalKnowledgeIntelligence.ArchitectureTests` verifies dependency direction.

Run:

```bash
dotnet test backend/LocalKnowledgeIntelligence.sln --no-build -m:1
```

Frontend build verification:

```bash
cd frontend
npm install
npm run build
```

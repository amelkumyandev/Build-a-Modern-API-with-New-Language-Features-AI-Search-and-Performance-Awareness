# Setup Guide

This guide explains how to configure and run the Local Knowledge Intelligence platform for local development.

## Prerequisites

- Docker Desktop, Rancher Desktop, or another Docker Compose compatible runtime.
- .NET 10 SDK for local backend build and test commands.
- Node.js with npm for local frontend build and smoke-test commands.
- An OpenAI API key with access to the configured chat and embedding models.

## Environment Configuration

From the repository root, create a local environment file:

```powershell
Copy-Item .env.example .env
```

For Bash-compatible shells:

```bash
cp .env.example .env
```

Set these values in `.env`:

```env
OPENAI_API_KEY=your_openai_api_key
JWT_SIGNING_KEY=replace_with_a_long_random_local_secret
POSTGRES_USER=lki
POSTGRES_PASSWORD=lki
POSTGRES_DB=lki
POSTGRES_PORT=5432
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

Security notes:

- Do not commit `.env`.
- Do not paste real tokens into documentation, logs, tests, or screenshots.
- Replace the default `JWT_SIGNING_KEY` before any shared or deployed environment.
- The seeded `admin` / `admin` account is for local development only.

## Start the Full Stack

Run the application with Docker Compose:

```powershell
docker compose up --build -d
```

The stack starts three services:

| Service | Container | Local URL or port |
| --- | --- | --- |
| Frontend | `lki-frontend` | http://localhost:3000 |
| Backend API | `lki-backend` | http://localhost:5000 |
| PostgreSQL with pgvector | `lki-postgres` | `localhost:5432` |

Check container status:

```powershell
docker compose ps
```

Verify the backend:

```powershell
Invoke-RestMethod http://localhost:5000/health
Invoke-RestMethod http://localhost:5000/health/ready
```

Open:

- Frontend: http://localhost:3000
- Swagger: http://localhost:5000/swagger
- Login: `admin` / `admin`

## First-Run Data Setup

After login:

1. Open the Dashboard.
2. Select **Generate seed data**.
3. Select **Re-index all documents**.

The seed catalog creates demo documents and evaluation questions. Re-indexing chunks the seeded documents and sends embedding requests to OpenAI. With the current seed set, a successful seed and re-index produces at least 75 chunks.

You can also run the same workflow through the API:

```powershell
$login = Invoke-RestMethod `
  -Uri 'http://localhost:5000/api/auth/login' `
  -Method Post `
  -ContentType 'application/json' `
  -Body (@{ username = 'admin'; password = 'admin' } | ConvertTo-Json)

$headers = @{ Authorization = "Bearer $($login.accessToken)" }

Invoke-RestMethod -Uri 'http://localhost:5000/api/admin/seed' -Method Post -Headers $headers
Invoke-RestMethod -Uri 'http://localhost:5000/api/admin/reindex' -Method Post -Headers $headers
```

## Backend Local Commands

Use these commands when working on the .NET solution outside Docker:

```powershell
dotnet restore backend\LocalKnowledgeIntelligence.sln
dotnet build backend\LocalKnowledgeIntelligence.sln -m:1
dotnet test backend\LocalKnowledgeIntelligence.sln --no-build -m:1
dotnet run --project backend\src\LocalKnowledgeIntelligence.Api
```

The `-m:1` flag keeps MSBuild project-reference builds single-threaded. This avoids a local SDK diagnostic issue observed on this machine with parallel solution builds.

## Frontend Local Commands

Use these commands when working on the Next.js frontend outside Docker:

```powershell
Set-Location frontend
npm install
npm run build
npm run dev
```

With the Docker stack running, execute the frontend smoke test:

```powershell
Set-Location frontend
npm run test:smoke
```

## Stop or Reset

Stop containers while keeping the database volume:

```powershell
docker compose down
```

Stop containers and delete local PostgreSQL data:

```powershell
docker compose down -v
```

Use `down -v` only when you intentionally want a clean database.

## Troubleshooting

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| Semantic search, re-indexing, or agent answers return setup errors | `OPENAI_API_KEY` is missing or invalid | Update `.env`, then restart with `docker compose up --build -d` |
| Frontend cannot reach the API | API URL mismatch or backend not running | Confirm `NEXT_PUBLIC_API_BASE_URL=http://localhost:5000` and check `docker compose ps` |
| Port is already in use | Another local service uses `3000`, `5000`, or `5432` | Stop the other service or adjust the Compose port mapping |
| Seeded documents have no embeddings | Seed data was created before re-indexing | Run **Re-index all documents** from the Dashboard |
| Database state looks stale | PostgreSQL volume preserved old data | Run `docker compose down -v`, then start and seed again |


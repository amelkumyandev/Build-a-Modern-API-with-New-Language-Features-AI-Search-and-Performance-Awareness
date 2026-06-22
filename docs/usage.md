# Usage Guide

This guide explains how to use the Local Knowledge Intelligence platform after it is running.

## Sign In

Open http://localhost:3000 and sign in with:

```text
Username: admin
Password: admin
```

The default account is intended for local development only.

## Dashboard

The Dashboard is the operational starting point. It shows platform health, document counts, chunk and embedding counts, failed embeddings, OpenAI configuration status, and the latest evaluation score.

Common actions:

- **Generate seed data** creates demo knowledge documents and evaluation questions.
- **Re-index all documents** chunks active documents and generates embeddings.
- **Run evaluation** executes the retrieval evaluation suite.
- Quick links open document creation, search, agent chat, and evaluation views.

Recommended first workflow:

1. Generate seed data.
2. Re-index all documents.
3. Open Search and run a hybrid query.
4. Open Agent and ask a grounded question.
5. Run Evaluation to inspect retrieval quality.

## Documents

The Documents view is used to manage local knowledge records.

Available workflows:

- List documents with paging and tag filtering.
- Create a document.
- Create a document and immediately index it.
- Open document details.
- Chunk, embed, or re-index an individual document.
- View generated chunks and embedding status.
- Delete a document.

When creating a document, use **Save and embed** when you want the document to become searchable immediately. This uses `POST /api/documents?index=true` and performs creation, chunking, and embedding in one workflow.

Use regular **Save** when you want to store a draft first and index it later from the document detail page or Dashboard.

## Search

The Search view supports three retrieval modes:

| Mode | Use when |
| --- | --- |
| Keyword | You need exact term matching or quick lexical checks |
| Semantic | You need meaning-based retrieval using embeddings |
| Hybrid | You want combined keyword and semantic ranking |

Each result includes the source document, chunk snippet, score, and score details when available. Open the source document to inspect surrounding content, or send the result context into the Agent workflow.

## Agent

The Agent view provides grounded chat over local knowledge.

Controls:

- Search mode: keyword, semantic, or hybrid.
- Top K: number of retrieved chunks passed into the answer-generation step.
- New session: starts a fresh conversation.
- Session history: reopens previous conversations.

Responses include citations so you can trace answers back to source chunks. Agent run details show retrieval and answer-generation steps for debugging and evaluation.

## Evaluation

The Evaluation view measures retrieval quality against stored questions.

Available actions:

- Generate evaluation questions from seeded knowledge.
- Run evaluation with a selected search mode and top K.
- Review overall score and failed cases.
- Inspect per-question results, expected documents, expected keywords, retrieved chunks, and matched status.

Use evaluation after changing chunking, ranking weights, seed documents, or search behavior.

## Settings

The Settings view exposes runtime configuration that can be adjusted without editing code.

Editable settings include:

- Chat model.
- Default search limit.
- Keyword weight.
- Semantic weight.

Read-only settings include:

- Embedding model.
- Embedding dimensions.
- Chunk size.
- Chunk overlap.
- Maximum chunks per document.

Embedding settings are treated as infrastructure-level settings because changing model or dimensions affects stored vector compatibility.

## API Examples

Authenticate first:

```powershell
$login = Invoke-RestMethod `
  -Uri 'http://localhost:5000/api/auth/login' `
  -Method Post `
  -ContentType 'application/json' `
  -Body (@{ username = 'admin'; password = 'admin' } | ConvertTo-Json)

$headers = @{ Authorization = "Bearer $($login.accessToken)" }
```

Create, save, and embed a document:

```powershell
$document = @{
  title = 'Local AI Search Runbook'
  content = 'This runbook explains how local AI search uses chunking, embeddings, pgvector, hybrid ranking, citations, and evaluation to answer grounded questions from private documents.'
  tags = @('runbook', 'ai-search')
  metadata = @{ owner = 'platform'; environment = 'local' }
} | ConvertTo-Json -Depth 8

Invoke-RestMethod `
  -Uri 'http://localhost:5000/api/documents?index=true' `
  -Method Post `
  -Headers $headers `
  -ContentType 'application/json' `
  -Body $document
```

Run hybrid search:

```powershell
Invoke-RestMethod `
  -Uri 'http://localhost:5000/api/search/hybrid?query=hybrid%20ranking&limit=5' `
  -Headers $headers
```

Ask the agent:

```powershell
$chat = @{
  sessionId = $null
  message = 'How does the platform combine keyword and semantic search?'
  searchMode = 'Hybrid'
  topK = 5
} | ConvertTo-Json

Invoke-RestMethod `
  -Uri 'http://localhost:5000/api/agent/chat' `
  -Method Post `
  -Headers $headers `
  -ContentType 'application/json' `
  -Body $chat
```

Run evaluation:

```powershell
$run = @{
  searchMode = 'Hybrid'
  topK = 5
} | ConvertTo-Json

Invoke-RestMethod `
  -Uri 'http://localhost:5000/api/evaluation/run' `
  -Method Post `
  -Headers $headers `
  -ContentType 'application/json' `
  -Body $run
```

## Operational Notes

- OpenAI embedding and chat calls can incur API cost.
- Re-indexing regenerates embeddings for document chunks.
- Keep secrets in `.env` or an external secret provider, not in source control.
- Use Swagger at http://localhost:5000/swagger for interactive endpoint exploration.
- Use the smoke test when validating the browser-facing workflow after frontend changes.


# API

Anonymous:

- `POST /api/auth/login`
- `GET /health`
- `GET /health/ready`

Authenticated:

- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/admin/dashboard`
- `POST /api/admin/seed`
- `POST /api/admin/reindex`
- `GET /api/admin/settings`
- `PUT /api/admin/settings`
- `POST /api/documents`
- `GET /api/documents`
- `GET /api/documents/{id}`
- `PUT /api/documents/{id}`
- `DELETE /api/documents/{id}`
- `POST /api/documents/{id}/chunk`
- `POST /api/documents/{id}/embed`
- `POST /api/documents/{id}/reindex`
- `GET /api/documents/{id}/chunks`
- `GET /api/search/keyword`
- `GET /api/search/semantic`
- `GET /api/search/hybrid`
- `POST /api/agent/chat`
- `GET /api/agent/sessions`
- `GET /api/agent/sessions/{id}`
- `GET /api/agent/runs/{id}`
- `POST /api/evaluation/generate-questions`
- `GET /api/evaluation/questions`
- `POST /api/evaluation/run`
- `GET /api/evaluation/runs/{id}`

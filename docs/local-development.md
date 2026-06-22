# Local Development

1. Copy `.env.example` to `.env`.
2. Set `OPENAI_API_KEY` and change `JWT_SIGNING_KEY`.
3. Run `docker compose up --build`.
4. Open http://localhost:3000.
5. Log in with `admin` / `admin`.
6. Use Dashboard > Generate seed data.
7. Re-index documents after setting an OpenAI key.

The backend runs migrations automatically at startup. PostgreSQL uses the `pgvector/pgvector:pg17` image and initializes `vector` plus `pg_trgm`.

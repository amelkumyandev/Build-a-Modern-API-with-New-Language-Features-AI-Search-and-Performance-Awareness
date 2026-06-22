# Vector Search

Document chunks are stored in `document_chunks`. EF Core owns relational metadata, while the `embedding vector(1536)` column is created by migration SQL and written with parameterized raw SQL.

Semantic search flow:

1. Validate the query.
2. Generate a query embedding with OpenAI `text-embedding-3-small`.
3. Search `document_chunks.embedding` with cosine distance using `<=>`.
4. Return distance and `similarityScore = 1 - distance`.

Hybrid search combines vector score, normalized keyword score, and recency score:

```text
finalScore = 0.65 * vectorScore + 0.30 * keywordScoreNormalized + 0.05 * recencyScore
```

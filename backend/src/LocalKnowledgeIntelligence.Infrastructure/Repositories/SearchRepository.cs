using System.Data;
using System.Globalization;
using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;

namespace LocalKnowledgeIntelligence.Infrastructure;

public sealed class SearchRepository(AppDbContext db) : ISearchRepository
{
    public async Task<IReadOnlyList<SearchHit>> KeywordSearchAsync(string query, int limit, CancellationToken cancellationToken)
    {
        var normalized = query.Trim();
        var rows = await db.DocumentChunks
            .AsNoTracking()
            .Include(chunk => chunk.Document)
            .Where(chunk => chunk.Document != null && chunk.Document.DeletedAt == null)
            .ToArrayAsync(cancellationToken);

        return rows
            .Select(chunk => ScoreKeyword(chunk, normalized))
            .Where(hit => hit.KeywordScore > 0)
            .OrderByDescending(hit => hit.KeywordScore)
            .ThenByDescending(hit => hit.UpdatedAt)
            .Take(limit)
            .ToArray();
    }

    public async Task<IReadOnlyList<SearchHit>> SemanticSearchAsync(float[] embedding, int limit, CancellationToken cancellationToken)
    {
        if (embedding.Length == 0)
        {
            return [];
        }

        var vector = ToVectorLiteral(embedding);
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT dc.id, dc.document_id, d.title, left(dc.content, 500) AS snippet,
                   (dc.embedding <=> CAST(@embedding AS vector)) AS distance,
                   d.updated_at
            FROM document_chunks dc
            INNER JOIN documents d ON d.id = dc.document_id
            WHERE dc.embedding IS NOT NULL
              AND d.deleted_at IS NULL
            ORDER BY dc.embedding <=> CAST(@embedding AS vector)
            LIMIT @limit;
            """;
        AddParameter(command, "embedding", vector);
        AddParameter(command, "limit", limit);

        var hits = new List<SearchHit>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var distance = reader.GetDouble(4);
            var similarity = Math.Clamp(1 - distance, 0, 1);
            var updatedAt = reader.GetFieldValue<DateTimeOffset>(5);
            hits.Add(new SearchHit(
                reader.GetGuid(1),
                reader.GetGuid(0),
                reader.GetString(2),
                reader.GetString(3),
                similarity,
                0,
                0,
                similarity,
                distance,
                ["vector"],
                updatedAt));
        }

        return hits;
    }

    public async Task StoreEmbeddingAsync(Guid chunkId, float[] embedding, string model, int dimensions, DateTimeOffset generatedAt, CancellationToken cancellationToken)
    {
        var vector = ToVectorLiteral(embedding);
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE document_chunks
            SET embedding = CAST(@embedding AS vector),
                embedding_model = @model,
                embedding_dimensions = @dimensions,
                embedding_generated_at = @generatedAt,
                updated_at = @generatedAt
            WHERE id = @chunkId;
            """;
        AddParameter(command, "embedding", vector);
        AddParameter(command, "model", model);
        AddParameter(command, "dimensions", dimensions);
        AddParameter(command, "generatedAt", generatedAt);
        AddParameter(command, "chunkId", chunkId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SearchHit ScoreKeyword(DocumentChunk chunk, string query)
    {
        var document = chunk.Document ?? throw new InvalidOperationException("Document must be loaded for keyword scoring.");
        var matched = new List<string>();
        var score = 0d;

        if (document.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            matched.Add("title");
            score += 5;
        }

        if (document.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            matched.Add("tags");
            score += 4;
        }

        if (document.Summary?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
        {
            matched.Add("summary");
            score += 3;
        }

        if (document.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            matched.Add("document");
            score += 1;
        }

        if (chunk.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            matched.Add("chunk");
            score += 2;
        }

        return new SearchHit(document.Id, chunk.Id, document.Title, BuildSnippet(chunk.Content, query), 0, score, 0, score, null, matched, document.UpdatedAt);
    }

    private static string BuildSnippet(string content, string query)
    {
        var index = content.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return content.Length <= 500 ? content : content[..500];
        }

        var start = Math.Max(0, index - 180);
        var length = Math.Min(500, content.Length - start);
        return content.Substring(start, length);
    }

    private static string ToVectorLiteral(float[] embedding)
    {
        return $"[{string.Join(",", embedding.Select(value => value.ToString("R", CultureInfo.InvariantCulture)))}]";
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

using System.Text;
using LocalKnowledgeIntelligence.Domain;

namespace LocalKnowledgeIntelligence.Application;

public sealed class DocumentChunkingService(ChunkingOptions options)
{
    public IReadOnlyList<ChunkDraft> Chunk(KnowledgeDocument document)
    {
        var paragraphs = document.Content
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(paragraph => paragraph.Length > 0)
            .ToArray();

        if (paragraphs.Length == 0)
        {
            paragraphs = [document.Content];
        }

        var chunks = new List<ChunkDraft>();
        var builder = new StringBuilder();
        var tokenEstimate = 0;
        var index = 0;
        var overlapBuffer = new Queue<string>();

        foreach (var paragraph in paragraphs)
        {
            var paragraphTokens = EstimateTokens(paragraph);
            if (builder.Length > 0 && tokenEstimate + paragraphTokens > options.TargetTokenCount)
            {
                chunks.Add(ToDraft(index++, builder.ToString(), tokenEstimate, document));
                builder.Clear();
                tokenEstimate = 0;

                foreach (var overlap in overlapBuffer)
                {
                    builder.AppendLine(overlap);
                    tokenEstimate += EstimateTokens(overlap);
                }
            }

            builder.AppendLine(paragraph);
            tokenEstimate += paragraphTokens;
            overlapBuffer.Enqueue(paragraph);

            while (overlapBuffer.Sum(EstimateTokens) > options.OverlapTokenCount && overlapBuffer.Count > 1)
            {
                overlapBuffer.Dequeue();
            }
        }

        if (builder.Length > 0)
        {
            chunks.Add(ToDraft(index, builder.ToString(), tokenEstimate, document));
        }

        return chunks.Count == 0 ? [ToDraft(0, document.Content, EstimateTokens(document.Content), document)] : chunks;
    }

    public static int EstimateTokens(string text)
    {
        return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
    }

    private static ChunkDraft ToDraft(int index, string content, int tokenEstimate, KnowledgeDocument document)
    {
        return new(index, content.Trim(), tokenEstimate, new Dictionary<string, object?>
        {
            ["documentTitle"] = document.Title,
            ["chunkIndex"] = index,
            ["sourceType"] = document.SourceType
        });
    }
}

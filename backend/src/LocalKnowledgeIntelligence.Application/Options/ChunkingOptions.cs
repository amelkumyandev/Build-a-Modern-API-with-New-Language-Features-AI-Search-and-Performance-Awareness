namespace LocalKnowledgeIntelligence.Application;

public sealed class ChunkingOptions
{
    public int TargetTokenCount
    {
        get;
        set => field = Math.Max(1, value);
    } = 850;

    public int OverlapTokenCount
    {
        get;
        set => field = Math.Max(0, value);
    } = 125;
}

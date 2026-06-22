namespace LocalKnowledgeIntelligence.Application;

public sealed class SearchOptions
{
    public int DefaultLimit
    {
        get;
        set => field = Math.Max(1, value);
    } = 10;

    public int MaxLimit
    {
        get;
        set => field = Math.Max(1, value);
    } = 50;

    public double VectorWeight
    {
        get;
        set => field = Math.Clamp(value, 0, 1);
    } = 0.65;

    public double KeywordWeight
    {
        get;
        set => field = Math.Clamp(value, 0, 1);
    } = 0.30;

    public double RecencyWeight
    {
        get;
        set => field = Math.Clamp(value, 0, 1);
    } = 0.05;
}

using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Application;

public sealed class RuntimeSettings(OpenAiOptions openAi, ChunkingOptions chunking, SearchOptions search)
{
    private readonly System.Threading.Lock _gate = new();
    private string _chatModel = openAi.ChatModel;
    private int _defaultSearchLimit = Math.Clamp(search.DefaultLimit, 1, search.MaxLimit);
    private double _vectorWeight = search.VectorWeight;
    private double _keywordWeight = search.KeywordWeight;
    private double _recencyWeight = search.RecencyWeight;

    public string EmbeddingModel => openAi.EmbeddingModel;
    public int EmbeddingDimensions => openAi.EmbeddingDimensions;
    public int TargetTokenCount => chunking.TargetTokenCount;
    public int OverlapTokenCount => chunking.OverlapTokenCount;
    public int MaxSearchLimit => search.MaxLimit;

    public string ChatModel
    {
        get { lock (_gate) return _chatModel; }
    }

    public int DefaultSearchLimit
    {
        get { lock (_gate) return _defaultSearchLimit; }
    }

    public SearchOptions SearchOptions
    {
        get
        {
            lock (_gate)
            {
                return new SearchOptions
                {
                    DefaultLimit = _defaultSearchLimit,
                    MaxLimit = search.MaxLimit,
                    VectorWeight = _vectorWeight,
                    KeywordWeight = _keywordWeight,
                    RecencyWeight = _recencyWeight
                };
            }
        }
    }

    public SettingsResponse ToResponse()
    {
        lock (_gate)
        {
            return new SettingsResponse(
                openAi.EmbeddingModel,
                _chatModel,
                openAi.EmbeddingDimensions,
                chunking.TargetTokenCount,
                chunking.OverlapTokenCount,
                _defaultSearchLimit,
                search.MaxLimit,
                _vectorWeight,
                _keywordWeight,
                _recencyWeight);
        }
    }

    public SettingsResponse Update(UpdateSettingsRequest request)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(request.ChatModel))
            {
                _chatModel = request.ChatModel.Trim();
            }

            if (request.DefaultSearchLimit is not null)
            {
                _defaultSearchLimit = Math.Clamp(request.DefaultSearchLimit.Value, 1, search.MaxLimit);
            }

            if (request.VectorWeight is not null)
            {
                _vectorWeight = Math.Clamp(request.VectorWeight.Value, 0, 1);
            }

            if (request.KeywordWeight is not null)
            {
                _keywordWeight = Math.Clamp(request.KeywordWeight.Value, 0, 1);
            }

            if (request.RecencyWeight is not null)
            {
                _recencyWeight = Math.Clamp(request.RecencyWeight.Value, 0, 1);
            }

            return ToResponse();
        }
    }
}

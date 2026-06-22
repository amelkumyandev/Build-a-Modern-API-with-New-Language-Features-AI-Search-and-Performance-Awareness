using System.Text;
using System.Text.Json;
using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Contracts;

namespace LocalKnowledgeIntelligence.Api;

public sealed record DocumentUploadInput(CreateDocumentRequest Request, bool Index);

public static class DocumentUploadFormReader
{
    private const long MaxUploadBytes = 200_000;
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md",
        ".markdown",
        ".json",
        ".csv",
        ".log"
    };

    public static async Task<DocumentUploadInput> ReadAsync(IFormCollection form, CancellationToken cancellationToken)
    {
        var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
        if (file is null)
        {
            throw ValidationFailureException.For("file", "A file field named 'file' is required.");
        }

        ValidateFile(file);

        var content = await ReadUtf8TextAsync(file, cancellationToken);
        var title = GetFormValue(form, "title");
        if (string.IsNullOrWhiteSpace(title))
        {
            title = Path.GetFileNameWithoutExtension(file.FileName);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Uploaded document";
        }

        var metadata = ParseMetadata(GetFormValue(form, "metadata"));
        metadata["source"] = "upload";
        metadata["fileName"] = Path.GetFileName(file.FileName);
        metadata["contentType"] = file.ContentType;
        metadata["sizeBytes"] = file.Length;

        var request = new CreateDocumentRequest(
            title.Trim(),
            content.Trim(),
            SplitTags(GetFormValue(form, "tags")),
            metadata);

        return new DocumentUploadInput(request, ParseBoolean(GetFormValue(form, "index")));
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw ValidationFailureException.For("file", "Uploaded file cannot be empty.");
        }

        if (file.Length > MaxUploadBytes)
        {
            throw ValidationFailureException.For("file", $"Uploaded file cannot exceed {MaxUploadBytes:N0} bytes.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!SupportedExtensions.Contains(extension))
        {
            throw ValidationFailureException.For(
                "file",
                "Supported upload formats are .txt, .md, .markdown, .json, .csv, and .log.");
        }
    }

    private static async Task<string> ReadUtf8TextAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, StrictUtf8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        catch (DecoderFallbackException)
        {
            throw ValidationFailureException.For("file", "Uploaded file must be valid UTF-8 text.");
        }
    }

    private static Dictionary<string, object?> ParseMetadata(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(raw, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            throw ValidationFailureException.For("metadata", "Metadata must be a valid JSON object.");
        }
    }

    private static IReadOnlyCollection<string> SplitTags(string? raw)
    {
        return raw?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
    }

    private static bool ParseBoolean(string? raw)
    {
        return bool.TryParse(raw, out var value) && value;
    }

    private static string? GetFormValue(IFormCollection form, string key)
    {
        return form.TryGetValue(key, out var value) ? value.ToString() : null;
    }
}

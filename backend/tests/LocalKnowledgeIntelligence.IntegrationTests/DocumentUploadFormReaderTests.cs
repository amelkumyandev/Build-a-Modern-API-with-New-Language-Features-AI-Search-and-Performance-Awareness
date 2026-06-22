using System.Text;
using LocalKnowledgeIntelligence.Api;
using LocalKnowledgeIntelligence.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace LocalKnowledgeIntelligence.IntegrationTests;

public sealed class DocumentUploadFormReaderTests
{
    [Fact]
    public async Task ReadAsync_ConvertsMultipartTextFileToCreateRequest()
    {
        var form = CreateForm(
            "local-rag-upload.md",
            string.Join(" ", Enumerable.Repeat("Uploaded markdown content for local RAG retrieval and document chunking.", 10)),
            new Dictionary<string, StringValues>
            {
                ["tags"] = "Upload, RAG",
                ["metadata"] = "{\"department\":\"engineering\"}",
                ["index"] = "true"
            });

        var upload = await DocumentUploadFormReader.ReadAsync(form, CancellationToken.None);

        Assert.True(upload.Index);
        Assert.Equal("local-rag-upload", upload.Request.Title);
        Assert.Contains("Uploaded markdown content", upload.Request.Content);
        Assert.Equal(["Upload", "RAG"], upload.Request.Tags);
        Assert.Equal("upload", upload.Request.Metadata?["source"]);
        Assert.Equal("local-rag-upload.md", upload.Request.Metadata?["fileName"]);
    }

    [Fact]
    public async Task ReadAsync_RejectsUnsupportedBinaryFormat()
    {
        var form = CreateForm("requirements.docx", "not real docx content", []);

        var ex = await Assert.ThrowsAsync<ValidationFailureException>(() =>
            DocumentUploadFormReader.ReadAsync(form, CancellationToken.None));

        Assert.Contains("file", ex.Errors.Keys);
    }

    private static FormCollection CreateForm(string fileName, string content, Dictionary<string, StringValues> fields)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
        var files = new FormFileCollection { file };
        return new FormCollection(fields, files);
    }
}

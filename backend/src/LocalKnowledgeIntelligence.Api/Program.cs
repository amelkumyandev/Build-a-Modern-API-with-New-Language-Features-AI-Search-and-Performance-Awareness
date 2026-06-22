using LocalKnowledgeIntelligence.Api;
using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
jwtOptions.SigningKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? jwtOptions.SigningKey;

var openAiOptions = builder.Configuration.GetSection("OpenAI").Get<OpenAiOptions>() ?? new OpenAiOptions();
openAiOptions.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? openAiOptions.ApiKey;

var chunkingOptions = builder.Configuration.GetSection("Chunking").Get<ChunkingOptions>() ?? new ChunkingOptions();
var searchOptions = builder.Configuration.GetSection("Search").Get<SearchOptions>() ?? new SearchOptions();

builder.Services.Configure<JwtOptions>(options =>
{
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.ExpirationHours = jwtOptions.ExpirationHours;
    options.SigningKey = jwtOptions.SigningKey;
});
builder.Services.Configure<OpenAiOptions>(options =>
{
    options.ApiKey = openAiOptions.ApiKey;
    options.EmbeddingModel = openAiOptions.EmbeddingModel;
    options.ChatModel = openAiOptions.ChatModel;
    options.EmbeddingDimensions = openAiOptions.EmbeddingDimensions;
});

builder.Services.AddSingleton(openAiOptions);
builder.Services.AddSingleton(chunkingOptions);
builder.Services.AddSingleton(searchOptions);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(jwtOptions);

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnablePersistAuthorization());
}

app.UseCors(ApiServiceCollectionExtensions.FrontendDevCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapLkiEndpoints(openAiOptions);
app.MapHealthChecks("/health", new HealthCheckOptions { AllowCachingResponses = false }).AllowAnonymous();
app.MapGet("/health/ready", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    var ready = await db.Database.CanConnectAsync(cancellationToken);
    return ready ? Results.Ok(new { status = "ready" }) : Results.Problem("Database is not reachable.", statusCode: StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing"))
{
    await DatabaseInitializer.InitializeAsync(app.Services);
    app.Logger.LogInformation("Local Knowledge Intelligence API started.");

    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogWarning("Development admin account admin/admin is enabled for local demo only.");
        if (jwtOptions.SigningKey == "local-dev-signing-key-change-me")
        {
            app.Logger.LogWarning("Default JWT_SIGNING_KEY is active. Change it before any real deployment.");
        }
    }
}

await app.RunAsync();

public partial class Program;

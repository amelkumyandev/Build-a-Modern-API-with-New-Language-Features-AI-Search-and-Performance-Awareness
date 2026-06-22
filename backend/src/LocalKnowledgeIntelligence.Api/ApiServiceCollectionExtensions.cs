using System.Text.Json.Serialization;
using LocalKnowledgeIntelligence.Application;
using LocalKnowledgeIntelligence.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace LocalKnowledgeIntelligence.Api;

/// <summary>
/// Registers the HTTP host concerns (JSON, OpenAPI/Swagger, JWT authentication,
/// authorization, CORS, health checks) for the API composition root.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    public const string FrontendDevCorsPolicy = "FrontendDev";

    public static IServiceCollection AddApiServices(this IServiceCollection services, JwtOptions jwtOptions)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.AllowDuplicateProperties = false;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddProblemDetails();
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT authorization header. Use /api/auth/login, copy accessToken, then enter: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = JwtSigningKey.Create(jwtOptions.SigningKey),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();
        services.AddCors(options =>
        {
            options.AddPolicy(FrontendDevCorsPolicy, policy =>
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
        services.AddHealthChecks();

        return services;
    }
}

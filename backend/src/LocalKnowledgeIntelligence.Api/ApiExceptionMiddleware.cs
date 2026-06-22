using LocalKnowledgeIntelligence.Application;
using Microsoft.AspNetCore.Mvc;

namespace LocalKnowledgeIntelligence.Api;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationFailureException ex)
        {
            logger.LogInformation("Validation failed for {Path}", context.Request.Path);
            await Results.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest, title: "Validation failed").ExecuteAsync(context);
        }
        catch (UnauthorizedApplicationException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
        }
        catch (ConfigurationException ex)
        {
            logger.LogWarning("Configuration error on {Path}: {Message}", context.Request.Path, ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable, "Configuration error", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error on {Path}", context.Request.Path);
            var detail = environment.IsDevelopment() ? ex.Message : "An unexpected error occurred.";
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Unexpected error", detail);
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(problem);
    }
}

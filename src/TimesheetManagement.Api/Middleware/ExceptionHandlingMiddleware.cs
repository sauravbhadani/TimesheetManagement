using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TimesheetManagement.Application.Exceptions;

namespace TimesheetManagement.Api.Middleware;

/// <summary>
/// Converts Application-layer exceptions into a consistent ProblemDetails response shape.
/// Anything not one of our known exception types is logged and returned as an opaque 500 —
/// callers never see raw exception details.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, problem) = Map(ex, context);

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogWarning(ex, "{ExceptionType} handling {Method} {Path}", ex.GetType().Name, context.Request.Method, context.Request.Path);
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
        }
    }

    private static (int StatusCode, object Problem) Map(Exception ex, HttpContext context)
    {
        var instance = context.Request.Path;

        return ex switch
        {
            ValidationAppException validation => (StatusCodes.Status400BadRequest, new ValidationProblemDetails(validation.Errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = instance
            }),
            NotFoundException notFound => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Title = "Not found",
                Detail = notFound.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = instance
            }),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Detail = forbidden.Message,
                Status = StatusCodes.Status403Forbidden,
                Instance = instance
            }),
            ConflictException conflict => (StatusCodes.Status409Conflict, new ProblemDetails
            {
                Title = "Conflict",
                Detail = conflict.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = instance
            }),
            _ => (StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = instance
            })
        };
    }
}

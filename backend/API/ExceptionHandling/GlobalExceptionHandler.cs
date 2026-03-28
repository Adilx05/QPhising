using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace QPhising.API.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            logger.LogWarning(
                exception,
                "Request validation failed for {Path}. ErrorCount: {ErrorCount}",
                httpContext.Request.Path,
                errors.Sum(entry => entry.Value.Length));

            var validationProblem = new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };

            await WriteProblemDetailsAsync(httpContext, validationProblem, cancellationToken);
            return true;
        }

        logger.LogError(exception, "Unhandled exception for {Path}", httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred."
        };

        await WriteProblemDetailsAsync(httpContext, problem, cancellationToken);
        return true;
    }

    private static Task WriteProblemDetailsAsync(HttpContext httpContext, ProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        return JsonSerializer.SerializeAsync(
            utf8Json: httpContext.Response.Body,
            value: problemDetails,
            inputType: problemDetails.GetType(),
            options: null,
            cancellationToken: cancellationToken);
    }
}

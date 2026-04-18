using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.CQRS.Commands.ProxyValidation;
using QPhising.Application.Exceptions;

namespace QPhising.Api.Middleware;

public sealed class ProblemDetailsExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsExceptionMiddleware> _logger;

    public ProblemDetailsExceptionMiddleware(RequestDelegate next, ILogger<ProblemDetailsExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        var (statusCode, title, type) = MapException(ex);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(ex, "Request {Method} {Path} failed with {StatusCode}.", httpContext.Request.Method, httpContext.Request.Path, statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = ex.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (ex is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        if (ex is ProxyContractDriftException proxyContractDriftException)
        {
            problemDetails.Extensions["status"] = proxyContractDriftException.ValidationResult.Status.ToString();
            problemDetails.Extensions["swaggerLastModifiedUtc"] = proxyContractDriftException.ValidationResult.SwaggerLastModifiedUtc;
            problemDetails.Extensions["proxyGeneratedAtUtc"] = proxyContractDriftException.ValidationResult.ProxyGeneratedAtUtc;
            problemDetails.Extensions["suggestedRegenerationCommand"] = proxyContractDriftException.ValidationResult.SuggestedRegenerationCommand;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (int StatusCode, string Title, string Type) MapException(Exception exception) =>
        exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Validation failure",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1"),
            ApplicationAuthorizationException authorizationException when authorizationException.Message.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) => (
                StatusCodes.Status401Unauthorized,
                "Authentication required",
                "https://tools.ietf.org/html/rfc9110#section-15.5.2"),
            ApplicationAuthorizationException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "https://tools.ietf.org/html/rfc9110#section-15.5.4"),
            ProxyContractDriftException => (
                StatusCodes.Status409Conflict,
                "Contract drift detected",
                "https://tools.ietf.org/html/rfc9110#section-15.5.10"),
            BadHttpRequestException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1"),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1")
        };
}

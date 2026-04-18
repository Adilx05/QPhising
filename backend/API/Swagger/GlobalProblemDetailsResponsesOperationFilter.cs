using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QPhising.Api.Swagger;

public sealed class GlobalProblemDetailsResponsesOperationFilter : IOperationFilter
{
    private static readonly IReadOnlyDictionary<string, string> DefaultProblemResponses = new Dictionary<string, string>
    {
        [StatusCodes.Status400BadRequest.ToString()] = "Bad Request",
        [StatusCodes.Status401Unauthorized.ToString()] = "Unauthorized",
        [StatusCodes.Status403Forbidden.ToString()] = "Forbidden",
        [StatusCodes.Status500InternalServerError.ToString()] = "Internal Server Error"
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);

        foreach (var (statusCode, description) in DefaultProblemResponses)
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                operation.Responses[statusCode] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new() { Schema = schema },
                        ["application/json"] = new() { Schema = schema }
                    }
                };

                continue;
            }

            if (string.IsNullOrWhiteSpace(response.Description))
            {
                response.Description = description;
            }

            response.Content ??= new Dictionary<string, OpenApiMediaType>();

            response.Content["application/problem+json"] = new OpenApiMediaType { Schema = schema };
            response.Content["application/json"] = new OpenApiMediaType { Schema = schema };
        }
    }
}

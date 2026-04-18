using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QPhising.Api.Swagger;

public sealed class AuthorizationMetadataOperationFilter : IOperationFilter
{
    private const string BearerSchemeName = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!RequiresAuthorization(context))
        {
            return;
        }

        operation.Responses.TryAdd(
            StatusCodes.Status401Unauthorized.ToString(),
            new OpenApiResponse { Description = "Unauthorized" });

        operation.Responses.TryAdd(
            StatusCodes.Status403Forbidden.ToString(),
            new OpenApiResponse { Description = "Forbidden" });

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        var bearerSchemeReference = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Id = BearerSchemeName,
                Type = ReferenceType.SecurityScheme
            }
        };

        var requirement = new OpenApiSecurityRequirement
        {
            [bearerSchemeReference] = Array.Empty<string>()
        };

        if (operation.Security.Any(existing => existing.Keys.Any(key => key.Reference?.Id == BearerSchemeName)))
        {
            return;
        }

        operation.Security.Add(requirement);
    }

    private static bool RequiresAuthorization(OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (endpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return false;
        }

        if (endpointMetadata.OfType<IAuthorizeData>().Any())
        {
            return true;
        }

        var methodInfo = context.MethodInfo;
        var hasAllowAnonymous = methodInfo.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any()
            || methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any() == true;

        if (hasAllowAnonymous)
        {
            return false;
        }

        return methodInfo.GetCustomAttributes(true).OfType<IAuthorizeData>().Any()
            || methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<IAuthorizeData>().Any() == true;
    }
}

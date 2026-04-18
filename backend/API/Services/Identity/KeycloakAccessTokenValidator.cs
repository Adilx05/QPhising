using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Domain.Identity.ValueObjects;

namespace QPhising.Api.Services.Identity;

public sealed class KeycloakAccessTokenValidator : IAccessTokenValidator
{
    private static readonly string[] SupportedRoleClaimSources =
    [
        ClaimTypes.Role,
        "role",
        "roles",
        "realm_access",
        "resource_access"
    ];

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _openIdConfigurationManager;
    private readonly IOptionsMonitor<JwtValidationOptions> _jwtValidationOptionsMonitor;
    private readonly ILogger<KeycloakAccessTokenValidator> _logger;

    public KeycloakAccessTokenValidator(
        IConfigurationManager<OpenIdConnectConfiguration> openIdConfigurationManager,
        IOptionsMonitor<JwtValidationOptions> jwtValidationOptionsMonitor,
        ILogger<KeycloakAccessTokenValidator> logger)
    {
        _openIdConfigurationManager = openIdConfigurationManager;
        _jwtValidationOptionsMonitor = jwtValidationOptionsMonitor;
        _logger = logger;
    }

    public async Task<ValidatedAccessTokenPrincipal> ValidateAsync(string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return ValidatedAccessTokenPrincipal.Invalid("Access token is required.");
        }

        var jwtOptions = _jwtValidationOptionsMonitor.CurrentValue;
        if (string.IsNullOrWhiteSpace(jwtOptions.Authority) || string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            return ValidatedAccessTokenPrincipal.Invalid("JWT authority or audience is not configured.");
        }

        try
        {
            var openIdConfiguration = await _openIdConfigurationManager.GetConfigurationAsync(cancellationToken);
            var tokenValidationParameters = CreateTokenValidationParameters(jwtOptions, openIdConfiguration);

            var principal = _jwtSecurityTokenHandler.ValidateToken(accessToken, tokenValidationParameters, out _);
            var identityClaims = ExtractIdentityClaims(principal.Claims);
            var subject = principal.FindFirstValue("sub");

            return new ValidatedAccessTokenPrincipal(
                IsValid: true,
                IsExpired: false,
                Subject: subject,
                Claims: identityClaims,
                FailureReason: null);
        }
        catch (SecurityTokenExpiredException exception)
        {
            _logger.LogInformation(exception, "Access token validation failed because the token is expired.");
            return ValidatedAccessTokenPrincipal.Invalid("Access token expired.", isExpired: true);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Access token validation failed.");
            return ValidatedAccessTokenPrincipal.Invalid("Access token validation failed.");
        }
    }

    private static TokenValidationParameters CreateTokenValidationParameters(
        JwtValidationOptions options,
        OpenIdConnectConfiguration openIdConfiguration)
    {
        var validIssuer = NormalizeAuthority(options.Authority);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = validIssuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = openIdConfiguration.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    private static IReadOnlyCollection<IdentityClaim> ExtractIdentityClaims(IEnumerable<Claim> claims)
    {
        var identityClaims = claims
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Type) && !string.IsNullOrWhiteSpace(claim.Value))
            .Select(claim => new IdentityClaim(claim.Type, claim.Value))
            .ToList();

        foreach (var nestedRoleClaim in ExtractNestedRoleClaims(claims))
        {
            identityClaims.Add(nestedRoleClaim);
        }

        return identityClaims;
    }

    private static IEnumerable<IdentityClaim> ExtractNestedRoleClaims(IEnumerable<Claim> claims)
    {
        foreach (var claim in claims.Where(c => SupportedRoleClaimSources.Contains(c.Type, StringComparer.OrdinalIgnoreCase)))
        {
            if (!LooksLikeJson(claim.Value))
            {
                continue;
            }

            using var document = JsonDocument.Parse(claim.Value);
            foreach (var parsedClaim in EnumerateNestedClaims(document.RootElement, claim.Type))
            {
                yield return parsedClaim;
            }
        }
    }

    private static IEnumerable<IdentityClaim> EnumerateNestedClaims(JsonElement element, string path)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return new IdentityClaim(path, value);
                    }
                }
                else
                {
                    foreach (var nestedClaim in EnumerateNestedClaims(item, path))
                    {
                        yield return nestedClaim;
                    }
                }
            }

            yield break;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var propertyPath = $"{path}.{property.Name}";
                foreach (var nestedClaim in EnumerateNestedClaims(property.Value, propertyPath))
                {
                    yield return nestedClaim;
                }
            }
        }
    }

    private static bool LooksLikeJson(string value) =>
        value.StartsWith('{') || value.StartsWith('[');

    private static string NormalizeAuthority(string authority)
    {
        var normalized = authority.TrimEnd('/');

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("JWT authority must be an absolute URI.");
        }

        return normalized;
    }
}

public sealed class JwtValidationOptions
{
    public string Authority { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;
}

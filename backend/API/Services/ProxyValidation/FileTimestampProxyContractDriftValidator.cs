using QPhising.Application.Contracts.Abstractions.ProxyValidation;
using QPhising.Application.Contracts.Responses.ProxyValidation;

namespace QPhising.Api.Services.ProxyValidation;

public sealed class FileTimestampProxyContractDriftValidator : IProxyContractDriftValidator
{
    private const string RegenerateProxyCommand = "./scripts/generate-proxy.sh";

    public Task<ProxyDriftValidationResult> ValidateAsync(
        string swaggerContractPath,
        string proxyGenerationStampPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (!File.Exists(swaggerContractPath))
            {
                return Task.FromResult(new ProxyDriftValidationResult(
                    ProxyDriftValidationStatus.ValidationError,
                    $"Swagger contract file was not found at '{swaggerContractPath}'.",
                    null,
                    null,
                    RegenerateProxyCommand));
            }

            var swaggerLastModifiedUtc = File.GetLastWriteTimeUtc(swaggerContractPath);

            if (!File.Exists(proxyGenerationStampPath))
            {
                return Task.FromResult(new ProxyDriftValidationResult(
                    ProxyDriftValidationStatus.ProxyMissing,
                    $"Proxy generation stamp file was not found at '{proxyGenerationStampPath}'. Run proxy generation before continuing.",
                    swaggerLastModifiedUtc,
                    null,
                    RegenerateProxyCommand));
            }

            var proxyGeneratedAtUtc = File.GetLastWriteTimeUtc(proxyGenerationStampPath);

            if (swaggerLastModifiedUtc <= proxyGeneratedAtUtc)
            {
                return Task.FromResult(new ProxyDriftValidationResult(
                    ProxyDriftValidationStatus.InSync,
                    "Proxy contracts are in sync with the Swagger source.",
                    swaggerLastModifiedUtc,
                    proxyGeneratedAtUtc,
                    RegenerateProxyCommand));
            }

            return Task.FromResult(new ProxyDriftValidationResult(
                ProxyDriftValidationStatus.SwaggerContractChanged,
                "Swagger contract changed after the most recent proxy generation.",
                swaggerLastModifiedUtc,
                proxyGeneratedAtUtc,
                RegenerateProxyCommand));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ProxyDriftValidationResult(
                ProxyDriftValidationStatus.ValidationError,
                $"Unable to validate proxy contract drift: {ex.Message}",
                null,
                null,
                RegenerateProxyCommand));
        }
    }
}

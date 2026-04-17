using QPhising.Application.Contracts.Responses.ProxyValidation;

namespace QPhising.Application.Contracts.Abstractions.ProxyValidation;

public interface IProxyContractDriftValidator
{
    Task<ProxyDriftValidationResult> ValidateAsync(
        string swaggerContractPath,
        string proxyGenerationStampPath,
        CancellationToken cancellationToken);
}

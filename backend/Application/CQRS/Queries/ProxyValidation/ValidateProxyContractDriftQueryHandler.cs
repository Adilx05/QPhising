using MediatR;
using Microsoft.Extensions.Logging;
using QPhising.Application.Contracts.Abstractions.ProxyValidation;
using QPhising.Application.Contracts.Responses.ProxyValidation;

namespace QPhising.Application.CQRS.Queries.ProxyValidation;

public sealed class ValidateProxyContractDriftQueryHandler : IRequestHandler<ValidateProxyContractDriftQuery, ProxyDriftValidationResult>
{
    private readonly IProxyContractDriftValidator _proxyContractDriftValidator;
    private readonly ILogger<ValidateProxyContractDriftQueryHandler> _logger;

    public ValidateProxyContractDriftQueryHandler(
        IProxyContractDriftValidator proxyContractDriftValidator,
        ILogger<ValidateProxyContractDriftQueryHandler> logger)
    {
        _proxyContractDriftValidator = proxyContractDriftValidator;
        _logger = logger;
    }

    public async Task<ProxyDriftValidationResult> Handle(ValidateProxyContractDriftQuery request, CancellationToken cancellationToken)
    {
        var result = await _proxyContractDriftValidator.ValidateAsync(
            request.SwaggerContractPath,
            request.ProxyGenerationStampPath,
            cancellationToken);

        if (result.IsInSync)
        {
            _logger.LogInformation(
                "Proxy drift validation passed for Swagger contract '{SwaggerContractPath}' and stamp '{ProxyGenerationStampPath}'.",
                request.SwaggerContractPath,
                request.ProxyGenerationStampPath);

            return result;
        }

        _logger.LogWarning(
            "Proxy drift validation failed with status {ValidationStatus}. Message: {ValidationMessage}",
            result.Status,
            result.Message);

        return result;
    }
}

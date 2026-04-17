using MediatR;
using Microsoft.Extensions.Logging;
using QPhising.Application.CQRS.Queries.ProxyValidation;

namespace QPhising.Application.CQRS.Commands.ProxyValidation;

public sealed class AssertProxyContractSyncCommandHandler : IRequestHandler<AssertProxyContractSyncCommand>
{
    private readonly ISender _sender;
    private readonly ILogger<AssertProxyContractSyncCommandHandler> _logger;

    public AssertProxyContractSyncCommandHandler(
        ISender sender,
        ILogger<AssertProxyContractSyncCommandHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Handle(AssertProxyContractSyncCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _sender.Send(
            new ValidateProxyContractDriftQuery(request.SwaggerContractPath, request.ProxyGenerationStampPath),
            cancellationToken);

        if (validationResult.IsInSync)
        {
            _logger.LogInformation(
                "Proxy synchronization assertion succeeded for '{SwaggerContractPath}'.",
                request.SwaggerContractPath);
            return;
        }

        _logger.LogError(
            "Proxy synchronization assertion failed. Status: {Status}. Suggestion: {Command}",
            validationResult.Status,
            validationResult.SuggestedRegenerationCommand);

        throw new ProxyContractDriftException(validationResult);
    }
}

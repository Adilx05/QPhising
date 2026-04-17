using MediatR;

namespace QPhising.Application.CQRS.Commands.ProxyValidation;

public sealed record AssertProxyContractSyncCommand(
    string SwaggerContractPath,
    string ProxyGenerationStampPath) : IRequest;

using MediatR;
using QPhising.Application.Contracts.Responses.ProxyValidation;

namespace QPhising.Application.CQRS.Queries.ProxyValidation;

public sealed record ValidateProxyContractDriftQuery(
    string SwaggerContractPath,
    string ProxyGenerationStampPath) : IRequest<ProxyDriftValidationResult>;

using MediatR;
using QPhising.Application.Contracts.Responses.RuntimeConfiguration;

namespace QPhising.Application.CQRS.Queries.RuntimeConfiguration;

public sealed record GetRuntimeConfigurationQuery : IRequest<RuntimeConfigurationResult>;

using MediatR;

namespace QPhising.Application.Contracts.Abstractions.Persistence;

public interface ITransactionalRequest<out TResponse> : IRequest<TResponse>
{
}

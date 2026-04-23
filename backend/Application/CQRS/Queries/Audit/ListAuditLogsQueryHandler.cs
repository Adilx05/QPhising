using MediatR;
using QPhising.Application.Contracts.Abstractions.Audit;
using QPhising.Application.Contracts.Responses.Audit;

namespace QPhising.Application.CQRS.Queries.Audit;

public sealed class ListAuditLogsQueryHandler : IRequestHandler<ListAuditLogsQuery, AuditLogPageResult>
{
    private readonly IAuditLogReadRepository _auditLogReadRepository;

    public ListAuditLogsQueryHandler(IAuditLogReadRepository auditLogReadRepository)
    {
        _auditLogReadRepository = auditLogReadRepository;
    }

    public Task<AuditLogPageResult> Handle(ListAuditLogsQuery request, CancellationToken cancellationToken) =>
        _auditLogReadRepository.QueryAsync(
            new AuditLogFilter(
                request.FromUtc,
                request.ToUtc,
                request.Actor,
                request.Endpoint,
                request.OutcomeCode,
                request.CorrelationId,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDirection),
            cancellationToken);
}

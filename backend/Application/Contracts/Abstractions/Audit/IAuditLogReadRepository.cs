using QPhising.Application.Contracts.Responses.Audit;

namespace QPhising.Application.Contracts.Abstractions.Audit;

public interface IAuditLogReadRepository
{
    Task<AuditLogPageResult> QueryAsync(AuditLogFilter filter, CancellationToken cancellationToken);
}

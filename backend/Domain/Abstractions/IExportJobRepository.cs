using QPhising.Domain.Exports;

namespace QPhising.Domain.Abstractions;

public interface IExportJobRepository
{
    Task<ExportJob?> GetByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExportJob>> ListAsync(
        ExportJobReadCriteria criteria,
        CancellationToken cancellationToken = default);

    Task AddAsync(ExportJob exportJob, CancellationToken cancellationToken = default);

    void Update(ExportJob exportJob);

    Task<IReadOnlyCollection<ExportJob>> ListExpiredWithStoredFileAsync(
        DateTimeOffset asOfUtc,
        int take,
        CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Configuration;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class SystemSettingRepository(QPhisingDbContext dbContext) : ISystemSettingRepository
{
    public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return dbContext.SystemSettings.SingleOrDefaultAsync(setting => setting.Key == key, cancellationToken);
    }

    public Task AddAsync(SystemSetting systemSetting, CancellationToken cancellationToken = default)
    {
        return dbContext.SystemSettings.AddAsync(systemSetting, cancellationToken).AsTask();
    }

    public void Update(SystemSetting systemSetting)
    {
        dbContext.SystemSettings.Update(systemSetting);
    }
}

using QPhising.Domain.Configuration;

namespace QPhising.Domain.Abstractions;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task AddAsync(SystemSetting systemSetting, CancellationToken cancellationToken = default);

    void Update(SystemSetting systemSetting);
}

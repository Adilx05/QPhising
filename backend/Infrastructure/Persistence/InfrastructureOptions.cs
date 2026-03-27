namespace QPhising.Infrastructure.Persistence;

public sealed class InfrastructureOptions
{
    public string DatabaseConnectionString { get; init; } = string.Empty;
    public string RedisConnectionString { get; init; } = string.Empty;
}

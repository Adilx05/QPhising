namespace QPhising.Infrastructure.Persistence;

public sealed class InfrastructureOptions
{
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string RedisConnectionString { get; set; } = string.Empty;
}

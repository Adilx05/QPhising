namespace QPhising.Infrastructure.Persistence;

public sealed class InfrastructureOptions
{
    public const string SectionName = "Infrastructure";
    public required string ConnectionString { get; init; }
    public required string RedisConnectionString { get; init; }
}

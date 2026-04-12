namespace QPhising.Application.Features.Health;

public sealed record HealthStatus(string Service, DateTimeOffset TimestampUtc, string Status, string SetupStatus);

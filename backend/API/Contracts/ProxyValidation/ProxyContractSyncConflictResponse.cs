using QPhising.Application.Contracts.Responses.ProxyValidation;

namespace QPhising.Api.Contracts.ProxyValidation;

public sealed class ProxyContractSyncConflictResponse
{
    public required ProxyDriftValidationStatus Status { get; init; }

    public required string Message { get; init; }

    public DateTimeOffset? SwaggerLastModifiedUtc { get; init; }

    public DateTimeOffset? ProxyGeneratedAtUtc { get; init; }

    public required string SuggestedRegenerationCommand { get; init; }
}

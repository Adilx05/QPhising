namespace QPhising.Application.Contracts.Responses.ProxyValidation;

public sealed record ProxyDriftValidationResult(
    ProxyDriftValidationStatus Status,
    string Message,
    DateTimeOffset? SwaggerLastModifiedUtc,
    DateTimeOffset? ProxyGeneratedAtUtc,
    string SuggestedRegenerationCommand)
{
    public bool IsInSync => Status == ProxyDriftValidationStatus.InSync;
}

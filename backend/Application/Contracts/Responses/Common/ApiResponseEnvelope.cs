namespace QPhising.Application.Contracts.Responses.Common;

/// <summary>
/// Standard response envelope for successful API results.
/// </summary>
public sealed record ApiResponseEnvelope<TData>(
    TData Data,
    string? Message = null,
    string? CorrelationId = null)
    where TData : notnull;

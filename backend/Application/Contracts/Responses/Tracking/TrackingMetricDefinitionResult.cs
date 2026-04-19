namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingMetricDefinitionResult(
    string Metric,
    string Definition,
    string EdgeCaseBehavior);

using MediatR;
using QPhising.Application.Features.Tracking.ProcessTrackingClick;
using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution.Handlers;

public sealed class TrackingClickProcessingTaskHandler(IMediator mediator) : IQueuedTaskHandler
{
    public TaskType TaskType => TaskType.TrackingClickProcessing;

    public async Task<QueuedTaskHandlerResult> HandleAsync(QueuedTask queuedTask, CancellationToken cancellationToken)
    {
        string campaignIdValue = queuedTask.Payload.GetRequired("campaignId");
        string trackingCode = queuedTask.Payload.GetRequired("trackingCode");
        string clickedAtValue = queuedTask.Payload.GetRequired("clickedAt");

        if (!Guid.TryParse(campaignIdValue, out Guid campaignId))
        {
            return QueuedTaskHandlerResult.Failure(
                $"Task payload campaignId '{campaignIdValue}' is invalid.",
                isRetryable: false);
        }

        if (!DateTimeOffset.TryParse(clickedAtValue, out DateTimeOffset clickedAtUtc))
        {
            return QueuedTaskHandlerResult.Failure(
                $"Task payload clickedAt '{clickedAtValue}' is invalid.",
                isRetryable: false);
        }

        string ipAddress = queuedTask.Payload.Values.TryGetValue("ipAddress", out string? payloadIpAddress)
            ? payloadIpAddress
            : "0.0.0.0";

        string userAgent = queuedTask.Payload.Values.TryGetValue("userAgent", out string? payloadUserAgent)
            ? payloadUserAgent
            : "task-worker";

        queuedTask.Payload.Values.TryGetValue("fingerprint", out string? fingerprint);

        var result = await mediator.Send(
            new ProcessTrackingClickCommand(
                campaignId,
                trackingCode,
                ipAddress,
                userAgent,
                fingerprint,
                clickedAtUtc),
            cancellationToken);

        return result.IsSuccess
            ? QueuedTaskHandlerResult.Success()
            : QueuedTaskHandlerResult.Failure(string.Join(" | ", result.Errors), isRetryable: false);
    }
}

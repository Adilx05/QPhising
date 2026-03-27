using MediatR;
using QPhising.Application.Features.Tracking.GenerateTrackingLink;
using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution.Handlers;

public sealed class TrackingLinkGenerationTaskHandler(IMediator mediator) : IQueuedTaskHandler
{
    public TaskType TaskType => TaskType.TrackingLinkGeneration;

    public async Task<QueuedTaskHandlerResult> HandleAsync(QueuedTask queuedTask, CancellationToken cancellationToken)
    {
        string campaignIdValue = queuedTask.Payload.GetRequired("campaignId");
        string recipientEmail = queuedTask.Payload.GetRequired("recipientEmail");

        if (!Guid.TryParse(campaignIdValue, out Guid campaignId))
        {
            return QueuedTaskHandlerResult.Failure(
                $"Task payload campaignId '{campaignIdValue}' is invalid.",
                isRetryable: false);
        }

        var result = await mediator.Send(new GenerateTrackingLinkCommand(campaignId, recipientEmail), cancellationToken);
        return result.IsSuccess
            ? QueuedTaskHandlerResult.Success()
            : QueuedTaskHandlerResult.Failure(string.Join(" | ", result.Errors), isRetryable: false);
    }
}

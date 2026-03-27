using MediatR;
using QPhising.Application.Features.Campaigns.ActivateCampaign;
using QPhising.Domain.Tasks;

namespace QPhising.Worker.TaskExecution.Handlers;

public sealed class CampaignActivationTaskHandler(IMediator mediator) : IQueuedTaskHandler
{
    public TaskType TaskType => TaskType.CampaignActivation;

    public async Task<QueuedTaskHandlerResult> HandleAsync(QueuedTask queuedTask, CancellationToken cancellationToken)
    {
        string campaignIdValue = queuedTask.Payload.GetRequired("campaignId");

        if (!Guid.TryParse(campaignIdValue, out Guid campaignId))
        {
            return QueuedTaskHandlerResult.Failure(
                $"Task payload campaignId '{campaignIdValue}' is invalid.",
                isRetryable: false);
        }

        var result = await mediator.Send(new ActivateCampaignCommand(campaignId), cancellationToken);
        return result.IsSuccess
            ? QueuedTaskHandlerResult.Success()
            : QueuedTaskHandlerResult.Failure(string.Join(" | ", result.Errors), isRetryable: false);
    }
}

using QPhising.Domain.Tasks;
using QPhising.Domain.Tasks.Exceptions;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TaskAggregateUnitTests
{
    [Fact]
    public void Create_Should_Require_Payload_Contract_For_TaskType()
    {
        // Missing recipientEmail
        IReadOnlyDictionary<string, string> payload = new Dictionary<string, string>
        {
            ["campaignId"] = Guid.NewGuid().ToString()
        };

        Assert.Throws<TaskValidationException>(() =>
            QueuedTask.Create(TaskType.TrackingLinkGeneration, payload, maxAttempts: 3));
    }

    [Fact]
    public void Lifecycle_Should_Allow_Queued_To_Claimed_To_Running_To_Succeeded()
    {
        IReadOnlyDictionary<string, string> payload = new Dictionary<string, string>
        {
            ["campaignId"] = Guid.NewGuid().ToString(),
            ["trackingCode"] = "trk-001",
            ["clickedAt"] = DateTimeOffset.UtcNow.ToString("O")
        };

        QueuedTask task = QueuedTask.Create(TaskType.TrackingClickProcessing, payload, maxAttempts: 3);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        task.Claim(now.AddMinutes(5), now);
        task.StartExecution(now.AddSeconds(5));
        task.Complete(now.AddSeconds(10));

        Assert.Equal(TaskExecutionStatus.Succeeded, task.Status);
        Assert.Equal(1, task.AttemptCount);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void Requeue_Should_Be_Rejected_When_MaxAttempts_Exhausted()
    {
        IReadOnlyDictionary<string, string> payload = new Dictionary<string, string>
        {
            ["campaignId"] = Guid.NewGuid().ToString(),
            ["activateAt"] = DateTimeOffset.UtcNow.ToString("O")
        };

        QueuedTask task = QueuedTask.Create(TaskType.CampaignActivation, payload, maxAttempts: 1);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        task.Claim(now.AddMinutes(5), now);
        task.StartExecution(now.AddSeconds(1));
        task.Fail("transient failure", now.AddSeconds(2));

        TaskValidationException exception = Assert.Throws<TaskValidationException>(() => task.Requeue());
        Assert.Contains("exhausted max attempts", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RequeueForRetry_Should_Set_NextAttemptWindow()
    {
        IReadOnlyDictionary<string, string> payload = new Dictionary<string, string>
        {
            ["exportJobId"] = Guid.NewGuid().ToString(),
            ["exportType"] = "CampaignReport",
            ["format"] = "Excel",
            ["requestedByUserId"] = Guid.NewGuid().ToString(),
            ["requestedAt"] = DateTimeOffset.UtcNow.ToString("O")
        };

        QueuedTask task = QueuedTask.Create(TaskType.ExportGeneration, payload, maxAttempts: 3);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        task.Claim(now.AddMinutes(5), now);
        task.StartExecution(now.AddSeconds(1));
        task.Fail("temporary dependency outage", now.AddSeconds(2));

        DateTimeOffset nextAttemptAt = now.AddMinutes(2);
        task.RequeueForRetry(nextAttemptAt, now.AddSeconds(2));

        Assert.Equal(TaskExecutionStatus.Queued, task.Status);
        Assert.Equal(nextAttemptAt, task.NextAttemptAt);
    }

    [Fact]
    public void MoveToDeadLetter_Should_Require_Failed_Status()
    {
        IReadOnlyDictionary<string, string> payload = new Dictionary<string, string>
        {
            ["exportJobId"] = Guid.NewGuid().ToString(),
            ["exportType"] = "CampaignReport",
            ["format"] = "Excel",
            ["requestedByUserId"] = Guid.NewGuid().ToString(),
            ["requestedAt"] = DateTimeOffset.UtcNow.ToString("O")
        };

        QueuedTask task = QueuedTask.Create(TaskType.ExportGeneration, payload, maxAttempts: 2);

        Assert.Throws<InvalidTaskStatusTransitionException>(() => task.MoveToDeadLetter("invalid export"));
    }
}

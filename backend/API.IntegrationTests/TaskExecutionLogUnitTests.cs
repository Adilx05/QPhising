using QPhising.Domain.Tasks;
using QPhising.Domain.Tasks.Exceptions;

namespace QPhising.API.IntegrationTests;

public sealed class TaskExecutionLogUnitTests
{
    [Fact]
    public void Create_Should_Trim_CorrelationId_And_Details()
    {
        TaskExecutionLog log = TaskExecutionLog.Create(
            taskId: Guid.NewGuid(),
            eventType: TaskExecutionLogEventType.Started,
            taskStatus: TaskExecutionStatus.Running,
            attemptNumber: 1,
            correlationId: "  corr-123  ",
            details: "  execution started  ",
            executionDurationMilliseconds: 25);

        Assert.Equal("corr-123", log.CorrelationId);
        Assert.Equal("execution started", log.Details);
        Assert.Equal(25, log.ExecutionDurationMilliseconds);
    }

    [Fact]
    public void Create_Should_Reject_Negative_Duration()
    {
        Assert.Throws<TaskValidationException>(() =>
            TaskExecutionLog.Create(
                taskId: Guid.NewGuid(),
                eventType: TaskExecutionLogEventType.Failed,
                taskStatus: TaskExecutionStatus.Failed,
                attemptNumber: 1,
                executionDurationMilliseconds: -1));
    }
}

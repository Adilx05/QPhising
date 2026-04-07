namespace QPhising.Domain.Setup;

public sealed class SetupState
{
    private SetupState(bool isCompleted, DateTimeOffset? completedAtUtc)
    {
        IsCompleted = isCompleted;
        CompletedAtUtc = completedAtUtc;
    }

    private SetupState()
    {
    }

    public int Id { get; private set; } = 1;

    public bool IsCompleted { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static SetupState CreateInitial() => new(false, null);

    public void Finalize(DateTimeOffset completedAtUtc)
    {
        IsCompleted = true;
        CompletedAtUtc = completedAtUtc;
    }
}

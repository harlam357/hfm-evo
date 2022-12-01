namespace HFM.Core.ScheduledTasks;

public class DelegateScheduledTask : ScheduledTask
{
    public Action<CancellationToken> Action { get; }

    public DelegateScheduledTask(string name, double interval, Action<CancellationToken> action) : base(name)
    {
        Interval = interval;
        Action = action;
    }

    protected override void OnRun(CancellationToken ct) => Action(ct);
}

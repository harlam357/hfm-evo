namespace HFM.Core.ScheduledTasks;

public class ScheduledTaskChangedEventArgs : EventArgs
{
    public ScheduledTask Source { get; }

    public ScheduledTaskChangedAction Action { get; }

    public double? Interval { get; }

    public ScheduledTaskChangedEventArgs(ScheduledTask source, ScheduledTaskChangedAction action, double? interval)
    {
        Source = source;
        Action = action;
        Interval = interval;
    }

    public override string? ToString() => ToString(i => $"{i:#,##0} ms");

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public string? ToString(Func<double?, string> formatInterval)
    {
        string name = Source.Name;
        return Action switch
        {
            ScheduledTaskChangedAction.Started => $"{name} task scheduled: {formatInterval(Interval)}",
            ScheduledTaskChangedAction.Stopped => $"{name} task stopped",
            ScheduledTaskChangedAction.Running => $"{name} task running",
            ScheduledTaskChangedAction.Canceled => $"{name} task canceled",
            ScheduledTaskChangedAction.Faulted => $"{name} task faulted: {Interval:#,##0} ms {Source.Exception}",
            ScheduledTaskChangedAction.Finished => $"{name} task finished: {Interval:#,##0} ms",
            ScheduledTaskChangedAction.AlreadyInProgress => $"{name} task already in progress",
            _ => base.ToString()
        };
    }
}

namespace HFM.Core.Client;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public readonly record struct CompletedAndFailedWorkUnits
{
    /// <summary>
    /// Gets the number of completed units since the last client start.
    /// </summary>
    public int RunCompleted { get; init; }

    /// <summary>
    /// Gets the total number of completed units.
    /// </summary>
    public int TotalCompleted { get; init; }

    /// <summary>
    /// Gets the number of failed units since the last client start.
    /// </summary>
    public int RunFailed { get; init; }

    /// <summary>
    /// Gets the total number of failed units.
    /// </summary>
    public int TotalFailed { get; init; }

    public static CompletedAndFailedWorkUnits operator +(CompletedAndFailedWorkUnits left, CompletedAndFailedWorkUnits right) =>
        new()
        {
            RunCompleted = left.RunCompleted + right.RunCompleted,
            TotalCompleted = left.TotalCompleted + right.TotalCompleted,
            RunFailed = left.RunFailed + right.RunFailed,
            TotalFailed = left.TotalFailed + right.TotalFailed
        };

    public static CompletedAndFailedWorkUnits operator -(CompletedAndFailedWorkUnits left, CompletedAndFailedWorkUnits right) =>
        new()
        {
            RunCompleted = left.RunCompleted - right.RunCompleted,
            TotalCompleted = left.TotalCompleted - right.TotalCompleted,
            RunFailed = left.RunFailed - right.RunFailed,
            TotalFailed = left.TotalFailed - right.TotalFailed
        };
}

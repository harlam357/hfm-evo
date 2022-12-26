namespace HFM.Core.WorkUnits;

/// <summary>
/// Known work unit results
/// </summary>
public readonly record struct WorkUnitResult(int Value)
{
    /// <summary>
    /// No result
    /// </summary>
    public static WorkUnitResult None => new(0);
    /// <summary>
    /// Finished Unit (Terminating)
    /// </summary>
    public static WorkUnitResult FinishedUnit => new(1);
    /// <summary>
    /// Early Unit End (Terminating)
    /// </summary>
    public static WorkUnitResult EarlyUnitEnd => new(2);
    /// <summary>
    /// Unstable Machine (Terminating)
    /// </summary>
    public static WorkUnitResult UnstableMachine => new(3);
    /// <summary>
    /// Interrupted (Non-Terminating)
    /// </summary>
    public static WorkUnitResult Interrupted => new(4);
    /// <summary>
    /// Bad Work Unit (Terminating)
    /// </summary>
    public static WorkUnitResult BadWorkUnit => new(5);
    /// <summary>
    /// Core outdated (Non-Terminating)
    /// </summary>
    public static WorkUnitResult CoreOutdated => new(6);
    /// <summary>
    /// Client core communications error (Terminating)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static WorkUnitResult ClientCoreError => new(7);
    /// <summary>
    /// GPU memtest error (Non-Terminating)
    /// </summary>
    public static WorkUnitResult GpuMemtestError => new(8);
    /// <summary>
    /// Unknown Enum (Non-Terminating)
    /// </summary>
    public static WorkUnitResult UnknownEnum => new(9);
    /// <summary>
    /// Bad Frame Checksum (Terminating)
    /// </summary>
    public static WorkUnitResult BadFrameChecksum => new(10);

    public bool IsTerminating =>
        Value switch
        {
            1 => true,
            2 => true,
            3 => true,
            5 => true,
            7 => true,
            10 => true,
            _ => false
        };

    public override string ToString() =>
        Value switch
        {
            1 => WorkUnitResultString.FinishedUnit,
            2 => WorkUnitResultString.EarlyUnitEnd,
            3 => WorkUnitResultString.UnstableMachine,
            4 => WorkUnitResultString.Interrupted,
            5 => WorkUnitResultString.BadWorkUnit,
            6 => WorkUnitResultString.CoreOutdated,
            8 => WorkUnitResultString.GpuMemtestError,
            9 => WorkUnitResultString.UnknownEnum,
            10 => WorkUnitResultString.BadFrameChecksum,
            _ => WorkUnitResultString.Unknown
        };

    public static WorkUnitResult Parse(string? result) =>
        result switch
        {
            WorkUnitResultString.FinishedUnit => FinishedUnit,
            WorkUnitResultString.EarlyUnitEnd => EarlyUnitEnd,
            WorkUnitResultString.UnstableMachine => UnstableMachine,
            WorkUnitResultString.Interrupted => Interrupted,
            WorkUnitResultString.BadWorkUnit => BadWorkUnit,
            WorkUnitResultString.CoreOutdated => CoreOutdated,
            WorkUnitResultString.GpuMemtestError => GpuMemtestError,
            WorkUnitResultString.UnknownEnum => UnknownEnum,
            WorkUnitResultString.BadFrameChecksum => BadFrameChecksum,
            _ => None
        };

    private static class WorkUnitResultString
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        internal const string FinishedUnit = "FINISHED_UNIT";
        internal const string EarlyUnitEnd = "EARLY_UNIT_END";
        internal const string UnstableMachine = "UNSTABLE_MACHINE";
        internal const string Interrupted = "INTERRUPTED";
        internal const string BadWorkUnit = "BAD_WORK_UNIT";
        internal const string CoreOutdated = "CORE_OUTDATED";
        internal const string GpuMemtestError = "GPU_MEMTEST_ERROR";
        internal const string UnknownEnum = "UNKNOWN_ENUM";
        internal const string BadFrameChecksum = "BAD_FRAME_CHECKSUM";
        internal const string Unknown = "UNKNOWN";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }
}

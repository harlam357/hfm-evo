using HFM.Core.Collections;
using HFM.Log;

namespace HFM.Core.WorkUnits;

public record WorkUnit : IProjectInfo, IItemIdentifier
{
    /// <summary>
    /// Gets the universal time this work unit record was generated.
    /// </summary>
    public DateTime UnitRetrievalTime { get; init; }

    public string? DonorName { get; init; }

    public int DonorTeam { get; init; }

    /// <summary>
    /// Gets the work unit assigned date and time.
    /// </summary>
    public DateTime Assigned { get; init; }

    /// <summary>
    /// Gets the work unit timeout date and time.
    /// </summary>
    public DateTime Timeout { get; init; }

    /// <summary>
    /// Gets the time stamp of the start of the work unit.
    /// </summary>
    public TimeSpan UnitStartTimeStamp { get; init; }

    /// <summary>
    /// Gets the work unit finished date and time.
    /// </summary>
    public DateTime? Finished { get; init; }

    public Version? CoreVersion { get; init; }

    public int ProjectId { get; init; }

    public int ProjectRun { get; init; }

    public int ProjectClone { get; init; }

    public int ProjectGen { get; init; }

    public WorkUnitPlatform? Platform { get; init; }

    public WorkUnitResult Result { get; init; }

    /// <summary>
    /// Gets the number of frames observed (completed) since last unit start or resume from pause.
    /// </summary>
    public int FramesObserved { get; init; }

    /// <summary>
    /// Gets the last observed frame of this work unit.
    /// </summary>
    public LogLineFrameData? CurrentFrame
    {
        get
        {
            if (Frames == null || Frames.Count == 0)
            {
                return null;
            }

            int max = Frames.Keys.Max();
            return max >= 0 ? Frames[max] : null;
        }
    }

    public IReadOnlyCollection<LogLine>? LogLines { get; init; }

    /// <summary>
    /// Gets the dictionary of frame data parsed from log lines.
    /// </summary>
    public IReadOnlyDictionary<int, LogLineFrameData>? Frames { get; init; }

    /// <summary>
    /// Gets the core hex identifier.
    /// </summary>
    public string? Core { get; init; }

    /// <summary>
    /// Gets the work unit hex identifier.
    /// </summary>
    public string? UnitHex { get; init; }

    public int Id { get; init; } = ItemIdentifier.NoId;

    /// <summary>
    /// Gets the frame by frame ID or null if the ID does not exist.
    /// </summary>
    public LogLineFrameData? GetFrame(int frameId) =>
        Frames is not null && Frames.ContainsKey(frameId) ? Frames[frameId] : null;
}

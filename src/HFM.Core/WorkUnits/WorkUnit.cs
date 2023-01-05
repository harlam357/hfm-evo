using HFM.Core.Collections;
using HFM.Log;
using HFM.Proteins;

namespace HFM.Core.WorkUnits;

public record WorkUnit : IProjectInfo, IItemIdentifier
{
    public int Id { get; init; } = ItemIdentifier.NoId;

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

    /// <summary>
    /// Gets the core hex identifier.
    /// </summary>
    public string? Core { get; init; }

    public Version? CoreVersion { get; init; }

    public int ProjectId { get; init; }

    public int ProjectRun { get; init; }

    public int ProjectClone { get; init; }

    public int ProjectGen { get; init; }

    /// <summary>
    /// Gets the work unit hex identifier.
    /// </summary>
    public string? UnitHex { get; init; }

    public Protein? Protein { get; init; }

    public WorkUnitPlatform? Platform { get; init; }

    public WorkUnitResult Result { get; init; }

    public IReadOnlyCollection<LogLine>? LogLines { get; init; }

    /// <summary>
    /// Gets the dictionary of frame data parsed from log lines.
    /// </summary>
    public IReadOnlyDictionary<int, LogLineFrameData>? Frames { get; init; }

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
            if (Frames is null || Frames.Count == 0)
            {
                return null;
            }

            int max = Frames.Keys.Max();
            return max >= 0 ? Frames[max] : null;
        }
    }

    public int FramesComplete
    {
        get
        {
            var currentFrame = CurrentFrame;
            if (currentFrame is null || currentFrame.ID < 0 || Protein is null)
            {
                return 0;
            }

            return currentFrame.ID <= Protein.Frames
                ? currentFrame.ID
                : Protein.Frames;
        }
    }

    public bool ShouldUseBenchmarkFrameTime(PpdCalculation ppdCalculation) =>
        this.HasProject() && CalculateRawTime(ppdCalculation) == 0;

    public int Progress => Protein is null ? 0 : FramesComplete * 100 / Protein.Frames;

    /// <summary>
    /// Gets the frame by frame ID or null if the ID does not exist.
    /// </summary>
    public LogLineFrameData? GetFrame(int frameId) =>
        Frames is not null && Frames.ContainsKey(frameId) ? Frames[frameId] : null;

    /// <summary>
    /// Gets the frame time for the given PPD calculation.
    /// </summary>
    public TimeSpan CalculateFrameTime(PpdCalculation ppdCalculation)
    {
        int rawTime = CalculateRawTime(ppdCalculation);
        return TimeSpan.FromSeconds(rawTime);
    }

    /// <summary>
    /// Get the credit for the given PPD and bonus calculations.
    /// </summary>
    public double CalculateCredit(PpdCalculation ppdCalculation, BonusCalculation calculateBonus)
    {
        if (!Protein.IsValid(Protein))
        {
            return 0.0;
        }

        var frameTime = CalculateFrameTime(ppdCalculation);
#pragma warning disable IDE0072 // Add missing cases
        return calculateBonus switch
        {
            BonusCalculation.DownloadTime => Protein.GetBonusCredit(GetUnitTimeByDownloadTime(frameTime)),
            BonusCalculation.FrameTime => Protein.GetBonusCredit(GetUnitTimeByFrameTime(frameTime)),
            _ => Protein!.Credit
        };
#pragma warning restore IDE0072 // Add missing cases
    }

    /// <summary>
    /// Gets the units per day (UPD) for the given PPD calculation.
    /// </summary>
    public double CalculateUnitsPerDay(PpdCalculation ppdCalculation) =>
        Protein?.GetUPD(CalculateFrameTime(ppdCalculation)) ?? 0.0;

    /// <summary>
    /// Gets the points per day (PPD) for the given PPD and bonus calculations.
    /// </summary>
    public double CalculatePointsPerDay(PpdCalculation ppdCalculation, BonusCalculation calculateBonus)
    {
        if (!Protein.IsValid(Protein))
        {
            return 0.0;
        }

        var frameTime = CalculateFrameTime(ppdCalculation);
#pragma warning disable IDE0072 // Add missing cases
        return calculateBonus switch
        {
            BonusCalculation.DownloadTime => Protein.GetBonusPPD(frameTime, GetUnitTimeByDownloadTime(frameTime)),
            BonusCalculation.FrameTime => Protein.GetBonusPPD(frameTime, GetUnitTimeByFrameTime(frameTime)),
            _ => Protein.GetPPD(frameTime),
        };
#pragma warning restore IDE0072 // Add missing cases
    }

    /// <summary>
    /// Gets the estimated time of arrival (ETA) for the given PPD calculation.
    /// </summary>
    public TimeSpan CalculateEta(PpdCalculation ppdCalculation) => CalculateEta(CalculateFrameTime(ppdCalculation));

    /// <summary>
    /// Gets the estimated time of arrival (ETA) for the given frame time.
    /// </summary>
    private TimeSpan CalculateEta(TimeSpan frameTime) =>
        Protein is null
            ? TimeSpan.Zero
            : new TimeSpan((Protein.Frames - FramesComplete) * frameTime.Ticks);

    public bool AllFramesCompleted => Protein?.Frames == FramesComplete;

    private TimeSpan GetUnitTimeByDownloadTime(TimeSpan frameTime)
    {
        if (Assigned == default)
        {
            return TimeSpan.Zero;
        }

        if (Finished.HasValue)
        {
            return Finished.Value.Subtract(Assigned);
        }

        var eta = CalculateEta(frameTime);
        return eta == TimeSpan.Zero && AllFramesCompleted == false
            ? TimeSpan.Zero
            : UnitRetrievalTime.Add(eta).Subtract(Assigned);
    }

    private TimeSpan GetUnitTimeByFrameTime(TimeSpan frameTime) =>
        Protein is null
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(frameTime.TotalSeconds * Protein.Frames);

    /// <summary>
    /// Gets the raw frame time (in seconds) for the given PPD calculation.
    /// </summary>
    public int CalculateRawTime(PpdCalculation ppdCalculation) =>
        ppdCalculation switch
        {
            PpdCalculation.LastFrame => FramesObserved > 1 ? Convert.ToInt32(CurrentFrame?.Duration.TotalSeconds ?? 0.0) : 0,
            PpdCalculation.LastThreeFrames => FramesObserved > 3 ? GetDurationInSeconds(3) : 0,
            PpdCalculation.AllFrames => FramesObserved > 0 ? GetDurationInSeconds(FramesObserved) : 0,
            PpdCalculation.EffectiveRate => GetRawTimePerUnitDownload(),
            _ => 0
        };

    private int GetDurationInSeconds(int numberOfFrames)
    {
        var currentFrame = CurrentFrame;
        if (numberOfFrames < 1 || currentFrame is null)
        {
            return 0;
        }

        TimeSpan totalTime = TimeSpan.Zero;
        int countFrames = 0;

        int frameId = currentFrame.ID;
        for (int i = 0; i < numberOfFrames; i++)
        {
            var frame = GetFrame(frameId);
            if (frame is not null && frame.Duration > TimeSpan.Zero)
            {
                totalTime = totalTime.Add(frame.Duration);
                countFrames++;
            }
            frameId--;
        }

        return countFrames > 0
            ? Convert.ToInt32(totalTime.TotalSeconds) / countFrames
            : 0;
    }

    private int GetRawTimePerUnitDownload()
    {
        var currentFrame = CurrentFrame;
        if (currentFrame is null || currentFrame.ID <= 0 || Assigned == default)
        {
            return 0;
        }

        var timeSinceUnitAssignment = UnitRetrievalTime.Subtract(Assigned);
        return Convert.ToInt32(timeSinceUnitAssignment.TotalSeconds) / currentFrame.ID;
    }
}

using System.Globalization;

using HFM.Core.WorkUnits;
using HFM.Log;

namespace HFM.Core.Client;

public record ClientResource
{
    // TODO: might need to live at the Client level
    public string LogFileName => String.Format(CultureInfo.InvariantCulture, "{0}-{1}", ClientIdentifier.Name, "log.txt");

    public ClientIdentifier ClientIdentifier { get; init; }

    public ClientResourceStatus Status { get; init; }

    public CompletedAndFailedWorkUnits CompletedAndFailedWorkUnits { get; init; }

    public WorkUnit? WorkUnit { get; init; }

    public IReadOnlyCollection<LogLine>? LogLines { get; init; }

    public ClientPlatform? Platform { get; init; }

    public ClientResourceStatus CalculateStatus(PpdCalculation ppdCalculation)
    {
        var shouldUseBenchmarkFrameTime =
            WorkUnit is not null &&
            WorkUnit.ShouldUseBenchmarkFrameTime(ppdCalculation);

        return Status == ClientResourceStatus.Running
            ? shouldUseBenchmarkFrameTime
                ? ClientResourceStatus.RunningNoFrameTimes
                : ClientResourceStatus.Running
            : Status;
    }

    public int CalculateProgress(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning() || status == ClientResourceStatus.Paused
            ? WorkUnit?.Progress ?? 0
            : 0;
    }

    public virtual string? GetName() => ClientIdentifier.Name;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual string GetResourceType(bool showVersions) => String.Empty;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual string GetProcessor(bool showVersions) => String.Empty;

    public TimeSpan GetFrameTime(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.GetFrameTime(ppdCalculation) ?? TimeSpan.Zero
            : TimeSpan.Zero;
    }

    public double GetPpd(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.GetPpd(ppdCalculation, bonusCalculation) ?? 0.0
            : 0.0;
    }

    public double GetUpd(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.GetUpd(ppdCalculation) ?? 0.0
            : 0.0;
    }

    public double GetCredit(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.GetCredit(ppdCalculation, bonusCalculation) ?? 0.0
            : WorkUnit?.Protein?.Credit ?? 0.0;
    }

    public TimeSpan GetEta(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.GetEta(ppdCalculation) ?? TimeSpan.Zero
            : TimeSpan.Zero;
    }

    public DateTime GetEtaDate(PpdCalculation ppdCalculation)
    {
        var eta = GetEta(ppdCalculation);
        return eta == TimeSpan.Zero
            ? DateTime.MinValue
            : WorkUnit?.UnitRetrievalTime.Add(eta) ?? DateTime.MinValue;
    }

    public string GetCore(bool showVersions)
    {
        var core = WorkUnit?.Protein?.Core;
        var coreVersion = WorkUnit?.CoreVersion;

        return (showVersions && coreVersion is not null
            ? String.Format(CultureInfo.InvariantCulture, "{0} ({1})", core, coreVersion)
            : core) ?? String.Empty;
    }

    public string GetIdentity() =>
        String.IsNullOrWhiteSpace(WorkUnit?.DonorName)
            ? String.Empty
            : String.Format(CultureInfo.InvariantCulture, "{0} ({1})", WorkUnit.DonorName, WorkUnit.DonorTeam);

    public BonusCalculation NormalizeBonusCalculation(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        if (bonusCalculation != BonusCalculation.None)
        {
            // always use FrameTime bonus calculation when there are no real frame times
            bonusCalculation = status == ClientResourceStatus.RunningNoFrameTimes
                ? BonusCalculation.FrameTime
                : bonusCalculation;
        }
        return bonusCalculation;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static ClientResource Offline(ClientSettings settings) =>
        new()
        {
            ClientIdentifier = ClientIdentifier.FromSettings(settings),
            Status = ClientResourceStatus.Offline
        };
}

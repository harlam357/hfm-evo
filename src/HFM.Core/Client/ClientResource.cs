using System.Globalization;

using HFM.Core.WorkUnits;
using HFM.Log;

namespace HFM.Core.Client;

public record ClientResource
{
    private readonly PpdCalculation _ppdCalculation;
    private readonly BonusCalculation _bonusCalculation;
    private readonly bool _showVersions;
    private readonly bool _etaAsDate;

    // TODO: remove ClientResource default ctor
    public ClientResource() : this(default, default, default, default)
    {

    }

    public ClientResource(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation, bool showVersions, bool etaAsDate)
    {
        _ppdCalculation = ppdCalculation;
        _bonusCalculation = bonusCalculation;
        _showVersions = showVersions;
        _etaAsDate = etaAsDate;
    }

    // TODO: might need to live at the Client level
    public string LogFileName => String.Format(CultureInfo.InvariantCulture, "{0}-{1}", ClientIdentifier.Name, "log.txt");

    public ClientIdentifier ClientIdentifier { get; init; }

    public CompletedAndFailedWorkUnits CompletedAndFailedWorkUnits { get; init; }

    public WorkUnit? WorkUnit { get; init; }

    public IReadOnlyCollection<LogLine>? LogLines { get; init; }

    public ClientPlatform? Platform { get; init; }

    // calculated properties
    private readonly ClientResourceStatus? _status;

    public ClientResourceStatus Status
    {
        get => CalculateStatus(_ppdCalculation);
        init => _status = value;
    }

    private int? _progress;

    public int Progress => _progress ??= CalculateProgress(_ppdCalculation);

    public string? Name => FormatName();

    private string? _resourceType;

    public string ResourceType => _resourceType ??= FormatResourceType(_showVersions);

    private string? _processor;

    public string Processor => _processor ??= FormatProcessor(_showVersions);

    private TimeSpan? _frameTime;

    public TimeSpan FrameTime => _frameTime ??= CalculateFrameTime(_ppdCalculation);

    private double? _pointsPerDay;

    public double PointsPerDay => _pointsPerDay ??= CalculatePointsPerDay(_ppdCalculation, _bonusCalculation);

    private ClientResourceEtaValue? _etaValue;

    public ClientResourceEtaValue ETA => _etaValue ??= CalculateEtaValue(_ppdCalculation, _etaAsDate);

    private string? _core;

    public string Core => _core ??= FormatCore(_showVersions);

    private string? _projectRunCloneGen;

    public string ProjectRunCloneGen => _projectRunCloneGen ??= FormatProjectRunCloneGen();

    private double? _credit;

    public double Credit => _credit ??= CalculateCredit(_ppdCalculation, _bonusCalculation);

    private string? _donorIdentity;

    public string DonorIdentity => _donorIdentity ??= FormatDonorIdentity();

    private DateTime? _assigned;

    public DateTime Assigned => _assigned ??= CalculateAssigned();

    private DateTime? _timeout;

    public DateTime Timeout => _timeout ??= CalculateTimeout();

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual ClientResourceStatus CalculateStatus(PpdCalculation ppdCalculation) => _status ?? default;

    public int CalculateProgress(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning() || status == ClientResourceStatus.Paused
            ? WorkUnit?.Progress ?? 0
            : 0;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual string? FormatName() => ClientIdentifier.Name;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual string FormatResourceType(bool showVersions) => String.Empty;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public virtual string FormatProcessor(bool showVersions) => String.Empty;

    public TimeSpan CalculateFrameTime(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.CalculateFrameTime(ppdCalculation) ?? TimeSpan.Zero
            : TimeSpan.Zero;
    }

    public double CalculatePointsPerDay(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.CalculatePointsPerDay(ppdCalculation, bonusCalculation) ?? 0.0
            : 0.0;
    }

    public double CalculateUnitsPerDay(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.CalculateUnitsPerDay(ppdCalculation) ?? 0.0
            : 0.0;
    }

    public double CalculateCredit(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.CalculateCredit(ppdCalculation, bonusCalculation) ?? 0.0
            : WorkUnit?.Protein?.Credit ?? 0.0;
    }

    public ClientResourceEtaValue CalculateEtaValue(PpdCalculation ppdCalculation, bool etaAsDate)
    {
        TimeSpan eta = CalculateEta(ppdCalculation);
        DateTime? etaDate = etaAsDate ? CalculateEtaDate(ppdCalculation) : null;
        return new(eta, etaDate);
    }

    internal TimeSpan CalculateEta(PpdCalculation ppdCalculation)
    {
        var status = CalculateStatus(ppdCalculation);
        return status.IsRunning()
            ? WorkUnit?.CalculateEta(ppdCalculation) ?? TimeSpan.Zero
            : TimeSpan.Zero;
    }

    internal DateTime CalculateEtaDate(PpdCalculation ppdCalculation)
    {
        var eta = CalculateEta(ppdCalculation);
        return eta == TimeSpan.Zero
            ? DateTime.MinValue
            : WorkUnit?.UnitRetrievalTime.Add(eta) ?? DateTime.MinValue;
    }

    public string FormatCore(bool showVersions)
    {
        var core = WorkUnit?.Protein?.Core;
        var coreVersion = WorkUnit?.CoreVersion;

        return (showVersions && coreVersion is not null
            ? String.Format(CultureInfo.InvariantCulture, "{0} ({1})", core, coreVersion)
            : core) ?? String.Empty;
    }

    public string FormatProjectRunCloneGen() =>
        WorkUnit?.ToShortProjectString() ?? String.Empty;

    public string FormatDonorIdentity() =>
        String.IsNullOrWhiteSpace(WorkUnit?.DonorName)
            ? String.Empty
            : String.Format(CultureInfo.InvariantCulture, "{0} ({1})", WorkUnit.DonorName, WorkUnit.DonorTeam);

    public DateTime CalculateAssigned() => WorkUnit?.Assigned.ToLocalTime() ?? default;

    public DateTime CalculateTimeout() => WorkUnit?.Timeout.ToLocalTime() ?? default;

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
        new(default, default, default, default)
        {
            ClientIdentifier = ClientIdentifier.FromSettings(settings),
            Status = ClientResourceStatus.Offline
        };
}

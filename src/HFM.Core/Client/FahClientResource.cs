using System.Globalization;
using System.Text;

using HFM.Core.WorkUnits;

namespace HFM.Core.Client;

public record FahClientResource : ClientResource
{
    // TODO: remove FahClientResource default ctor
    public FahClientResource()
    {

    }

    public FahClientResource(PpdCalculation ppdCalculation, BonusCalculation bonusCalculation, bool showVersions, bool etaAsDate)
        : base(ppdCalculation, bonusCalculation, showVersions, etaAsDate)
    {

    }

    public ClientResourceStatus SlotStatus { get; init; }

    public FahClientSlotIdentifier SlotIdentifier { get; init; }

    public int SlotId { get; init; }

    public FahClientSlotDescription? SlotDescription { get; init; }

    private ClientResourceStatus? _status;

    public override ClientResourceStatus CalculateStatus(PpdCalculation ppdCalculation) =>
        _status ??= SlotStatus == ClientResourceStatus.Running
            ? ShouldUseBenchmarkFrameTime(ppdCalculation)
                ? ClientResourceStatus.RunningNoFrameTimes
                : ClientResourceStatus.Running
            : SlotStatus;

    private bool ShouldUseBenchmarkFrameTime(PpdCalculation ppdCalculation) =>
        WorkUnit is not null &&
        WorkUnit.ShouldUseBenchmarkFrameTime(ppdCalculation);

    public override string FormatName() => SlotIdentifier.Name;

    public override string FormatResourceType(bool showVersions)
    {
        if (SlotDescription is null || SlotDescription.SlotType == FahClientSlotType.Unknown)
        {
            return String.Empty;
        }

        var sb = new StringBuilder(SlotDescription.SlotType.ToString().ToUpperInvariant());
        if (Threads.HasValue)
        {
            sb.Append(CultureInfo.InvariantCulture, $":{Threads}");
        }
        if (showVersions && !String.IsNullOrEmpty(Platform?.ClientVersion))
        {
            sb.Append(CultureInfo.InvariantCulture, $" ({Platform.ClientVersion})");
        }
        return sb.ToString();
    }

    public int? Threads =>
        SlotDescription is FahClientCpuSlotDescription cpu
            ? cpu.CpuThreads
            : null;

    public override string FormatProcessor(bool showVersions)
    {
        var processor = SlotDescription?.Processor;
        var platform = WorkUnit?.Platform;
        bool showPlatform = platform?.Implementation
            is WorkUnitPlatformImplementation.CUDA
            or WorkUnitPlatformImplementation.OpenCL;

        return (showVersions && showPlatform
            ? $"{processor} ({platform!.Implementation} {platform.DriverVersion})"
            : processor) ?? String.Empty;
    }
}

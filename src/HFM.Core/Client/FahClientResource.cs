using System.Globalization;
using System.Text;

using HFM.Core.WorkUnits;

namespace HFM.Core.Client;

public record FahClientResource : ClientResource
{
    public FahClientSlotIdentifier SlotIdentifier { get; init; }

    public int SlotId { get; init; }

    public FahClientSlotDescription? SlotDescription { get; init; }

    public override string GetName() => SlotIdentifier.Name;

    public override string GetResourceType(bool showVersions)
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

    public override string GetProcessor(bool showVersions)
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

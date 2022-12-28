using System.Globalization;
using System.Text;

using HFM.Core.Client;
using HFM.Core.WorkUnits;
using HFM.Preferences;

namespace HFM.Console.ViewModels;

internal class FahClientResourceViewModel : ClientResourceViewModel
{
    private readonly FahClientResource _clientResource;

    public FahClientResourceViewModel(FahClientResource clientResource, IPreferences preferences) : base(clientResource, preferences)
    {
        _clientResource = clientResource;
    }

    public override string ResourceType
    {
        get
        {
            var r = _clientResource;
            if (r.SlotDescription is null || r.SlotDescription.SlotType == FahClientSlotType.Unknown)
            {
                return String.Empty;
            }

            var sb = new StringBuilder(r.SlotDescription.SlotType.ToString().ToUpperInvariant());
            if (Threads.HasValue)
            {
                sb.Append(CultureInfo.InvariantCulture, $":{Threads}");
            }
            if (ShowVersions && !String.IsNullOrEmpty(r.Platform?.ClientVersion))
            {
                sb.Append(CultureInfo.InvariantCulture, $" ({r.Platform.ClientVersion})");
            }
            return sb.ToString();
        }
    }

    public override string Processor
    {
        get
        {
            var processor = _clientResource.SlotDescription?.Processor;
            var platform = _clientResource.WorkUnit?.Platform;
            bool showPlatform = platform?.Implementation
                is WorkUnitPlatformImplementation.CUDA
                or WorkUnitPlatformImplementation.OpenCL;

            return (ShowVersions && showPlatform
                ? $"{processor} ({platform!.Implementation} {platform.DriverVersion})"
                : processor) ?? String.Empty;
        }
    }

    public int? Threads =>
        _clientResource.SlotDescription is FahClientCpuSlotDescription cpu
            ? cpu.CpuThreads
            : null;
}

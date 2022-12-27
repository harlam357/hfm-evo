using HFM.Core.WorkUnits;
using HFM.Log;

namespace HFM.Core.Client;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record ClientResource
{
    public ClientResourceStatus Status { get; init; }

    public int Progress { get; init; }

    public WorkUnit? WorkUnit { get; init; }

    public IReadOnlyCollection<LogLine>? LogLines { get; init; }

    public ClientPlatform? Platform { get; init; }
}

using HFM.Client;
using HFM.Client.ObjectModel;

namespace HFM.Core.Client;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record FahClientMessages
{
    public FahClientMessage? Heartbeat { get; init; }

    public Info? Info { get; init; }

    public Options? Options { get; init; }

    public SlotCollection? SlotCollection { get; init; }

    public UnitCollection? UnitCollection { get; init; }

    public IReadOnlyCollection<FahClientMessage>? ProcessedMessages { get; init; }
}

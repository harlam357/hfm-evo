using HFM.Client;
using HFM.Client.ObjectModel;
using HFM.Log;

namespace HFM.Core.Client;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record FahClientMessages
{
    public FahClientMessage? Heartbeat { get; init; }

    public Info? Info { get; init; }

    public Options? Options { get; init; }

    public SlotCollection? SlotCollection { get; init; }

    public UnitCollection? UnitCollection { get; init; }

    public ClientRun? ClientRun { get; init; }

    public IReadOnlyCollection<FahClientMessage>? ProcessedMessages { get; init; }

    public bool WasProcessed(string messageType) =>
        ProcessedMessages is not null && ProcessedMessages.Any(x => x.Identifier.MessageType == messageType);

    public bool LogMessageWasProcessed() =>
        ProcessedMessages is not null &&
        ProcessedMessages.Any(x => x.Identifier.MessageType is FahClientMessageType.LogRestart or FahClientMessageType.LogUpdate);
}

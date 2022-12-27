using System.Collections.Concurrent;
using System.Text;

using HFM.Client;
using HFM.Client.ObjectModel;
using HFM.Log;

namespace HFM.Core.Client;

public class FahClientMessageBuffer
{
    private readonly ConcurrentQueue<FahClientMessage> _messageQueue;

    public FahClientMessageBuffer()
    {
        _messageQueue = new ConcurrentQueue<FahClientMessage>();
    }

    public void Add(FahClientMessage message) =>
        _messageQueue.Enqueue(message);

    public bool ContainsAny(IEnumerable<string> messageTypes) =>
        _messageQueue.Any(x => messageTypes.Contains(x.Identifier.MessageType));

    private FahClientMessages _messages = new();

    public async Task<FahClientMessages> Empty()
    {
        var processedMessages = new List<FahClientMessage>();

        while (_messageQueue.TryDequeue(out FahClientMessage? message))
        {
            processedMessages.Add(message);
            await UpdateMessages(message).ConfigureAwait(false);
        }

        return _messages with { ProcessedMessages = processedMessages };
    }

    private async Task UpdateMessages(FahClientMessage message)
    {
        switch (message.Identifier.MessageType)
        {
            case FahClientMessageType.Heartbeat:
                _messages = _messages with { Heartbeat = message };
                break;
            case FahClientMessageType.Info:
                _messages = _messages with { Info = Info.Load(message.MessageText) };
                break;
            case FahClientMessageType.Options:
                _messages = _messages with { Options = Options.Load(message.MessageText) };
                break;
            case FahClientMessageType.SlotInfo:
                _messages = _messages with { SlotCollection = SlotCollection.Load(message.MessageText) };
                break;
            case FahClientMessageType.SlotOptions:
                UpdateSlotCollectionSlotOptions(_messages, message);
                break;
            case FahClientMessageType.QueueInfo:
                _messages = _messages with { UnitCollection = UnitCollection.Load(message.MessageText) };
                break;
            case FahClientMessageType.LogRestart:
            case FahClientMessageType.LogUpdate:
                await UpdateFahClientLog(message).ConfigureAwait(false);
                break;
            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }
    }

    private static void UpdateSlotCollectionSlotOptions(FahClientMessages messages, FahClientMessage message)
    {
        if (messages.SlotCollection is null)
        {
            return;
        }

        var slotOptions = SlotOptions.Load(message.MessageText);
        if (!Int32.TryParse(slotOptions[Options.MachineID], out var machineId))
        {
            return;
        }

        var slot = messages.SlotCollection.First(x => x.ID == machineId);
        slot.SlotOptions = slotOptions;
    }

    private readonly FahClientLog _log = new();

    private async Task UpdateFahClientLog(FahClientMessage message)
    {
        var logUpdate = LogUpdate.Load(message.MessageText);

        var logIsRetrieved = _log.ClientRuns.Count > 0;
        if (logIsRetrieved)
        {
            await UpdateFahClientLogFromStringBuilder(logUpdate.Value).ConfigureAwait(false);
        }
        else
        {
            AppendToLogBuffer(logUpdate.Value);
            if (message.MessageText.Length < UInt16.MaxValue)
            {
                await UpdateFahClientLogFromStringBuilder(_logBuffer!).ConfigureAwait(false);
                ReleaseLogBuffer();
            }
        }
    }

    private async Task UpdateFahClientLogFromStringBuilder(StringBuilder value)
    {
        using var textReader = new Internal.StringBuilderReader(value);
        using var reader = new FahClientLogTextReader(textReader);
        await _log.ReadAsync(reader).ConfigureAwait(false);

        if (_messages.ClientRun is null)
        {
            _messages = _messages with { ClientRun = _log.ClientRuns.LastOrDefault() };
        }
    }

    private StringBuilder? _logBuffer = new();

    private void AppendToLogBuffer(StringBuilder source)
    {
        foreach (var chunk in source.GetChunks())
        {
            _logBuffer!.Append(chunk);
        }
    }

    private void ReleaseLogBuffer() => _logBuffer = null;
}

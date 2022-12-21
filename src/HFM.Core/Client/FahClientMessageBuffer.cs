using System.Collections.Concurrent;

using HFM.Client;
using HFM.Client.ObjectModel;

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

    public FahClientMessages Empty()
    {
        var processedMessages = new List<FahClientMessage>();

        while (_messageQueue.TryDequeue(out FahClientMessage? message))
        {
            processedMessages.Add(message);
            UpdateMessages(message);
        }

        return _messages with { ProcessedMessages = processedMessages };
    }

    private void UpdateMessages(FahClientMessage message)
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
}

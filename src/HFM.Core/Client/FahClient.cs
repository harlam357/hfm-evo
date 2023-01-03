using System.Globalization;

using HFM.Client;
using HFM.Client.ObjectModel;
using HFM.Core.Internal;
using HFM.Core.Logging;
using HFM.Core.WorkUnits;
using HFM.Log;

namespace HFM.Core.Client;

public class FahClient : Client
{
    private readonly ILogger _logger;
    private readonly IProteinService _proteinService;

    public FahClient(ILogger? logger, IProteinService proteinService)
    {
        _logger = logger ?? NullLogger.Instance;
        _proteinService = proteinService;
    }

    protected FahClientConnection? Connection { get; set; }

    public override bool Connected => Connection is { Connected: true };

    protected override void OnClose()
    {
        if (Connected)
        {
            try
            {
                Connection!.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, Logger.NameFormat, Settings!.Name, ex.Message);
            }
        }

        ClearMessages();
    }

    public FahClientMessages? Messages { get; protected set; }

    protected FahClientMessageBuffer MessageBuffer { get; set; } = new();

    private void ClearMessages()
    {
        Messages = null;
        MessageBuffer = new FahClientMessageBuffer();
    }

    protected override async Task OnRefresh()
    {
        Messages = await MessageBuffer.Empty().ConfigureAwait(false);

        var messages = Messages;
        if (IsHeartbeatOverdue(messages.Heartbeat))
        {
            Close();
        }

        if (messages.WasProcessed(FahClientMessageType.SlotInfo))
        {
            foreach (var slot in messages.SlotCollection!)
            {
                await ExecuteSlotOptionsCommand(slot.ID).ConfigureAwait(false);
            }
        }

        if (messages.ClientRun is not null)
        {
            if (messages.LogMessageWasProcessed())
            {
                await ExecuteQueueInfoCommand().ConfigureAwait(false);
            }
            else
            {
                await OnProcessMessages(messages).ConfigureAwait(false);
            }
        }
    }

    private static bool IsHeartbeatOverdue(FahClientMessage? heartbeat)
    {
        if (heartbeat is null)
        {
            return false;
        }

        var minutesSinceLastHeartbeat = DateTime.UtcNow.Subtract(heartbeat.Identifier.Received).TotalMinutes;
        var maximumMinutesBetweenHeartbeats = TimeSpan.FromSeconds(HeartbeatInterval * 3).TotalMinutes;

        return minutesSinceLastHeartbeat > maximumMinutesBetweenHeartbeats;
    }

    private async Task OnProcessMessages(FahClientMessages messages)
    {
        if (messages.SlotCollection == null)
        {
            return;
        }

        var resources = new List<FahClientResource>();

        var clientIdentifier = ClientIdentifier;
        var workUnitCollectionBuilder = new FahClientWorkUnitCollectionBuilder(messages, _proteinService);
        foreach (var slot in messages.SlotCollection.Where(x => x.ID.HasValue))
        {
            var slotDescription = ParseSlotDescription(slot.Description, messages.Info);
            if (slotDescription is null)
            {
                continue;
            }

            int slotId = slot.ID!.Value;
            var previousWorkUnit = Resources?.Cast<FahClientResource>().FirstOrDefault(x => x.SlotId == slotId)?.WorkUnit;
            var workUnits = await workUnitCollectionBuilder.BuildForSlot(slotId, slotDescription, previousWorkUnit).ConfigureAwait(false);
            var currentWorkUnit = workUnits.Current;
            var status = (ClientResourceStatus)Enum.Parse(typeof(ClientResourceStatus), slot.Status, true);

            resources.Add(new FahClientResource
            {
                // FahClient
                SlotIdentifier = new FahClientSlotIdentifier(clientIdentifier, slotId),
                SlotId = slotId,
                SlotDescription = slotDescription,
                // Client
                ClientIdentifier = clientIdentifier,
                Status = status,
                // TODO: query work unit database for completed and failed values
                CompletedAndFailedWorkUnits = new CompletedAndFailedWorkUnits(),
                WorkUnit = currentWorkUnit,
                LogLines = EnumerateLogLines(messages.ClientRun, slotId, workUnits.Current),
                Platform = messages.Info is null ? null : new ClientPlatform(messages.Info.Client.Version, messages.Info.System.OS)
            });
        }

        SetResources(resources);
    }

    private static FahClientSlotDescription? ParseSlotDescription(string? description, Info? info)
    {
        var slotDescription = FahClientSlotDescription.Parse(description);
        if (slotDescription is null)
        {
            return null;
        }

        if (slotDescription.SlotType == FahClientSlotType.Cpu)
        {
            slotDescription.Processor = info?.System?.CPU;
        }
        return slotDescription;
    }

    private static IReadOnlyCollection<LogLine> EnumerateLogLines(ClientRun? clientRun, int slotId, WorkUnit? workUnit)
    {
        IEnumerable<LogLine>? logLines = workUnit?.LogLines;

        if (logLines is null)
        {
            var slotRun = clientRun?.GetSlotRun(slotId);
            if (slotRun is not null)
            {
                logLines = LogLineEnumerable.Create(slotRun);
            }
        }

        if (logLines is null && clientRun is not null)
        {
            logLines = LogLineEnumerable.Create(clientRun);
        }

        return logLines is null
            ? Array.Empty<LogLine>()
            : logLines.ToList();
    }

    protected Task? ReadMessagesTask { get; private set; }

    protected override async Task OnConnect()
    {
        await CreateAndOpenConnection().ConfigureAwait(false);

        ReadMessagesTask?.Dispose();

        ReadMessagesTask = Task.Run(async () =>
        {
            try
            {
                await ReadMessagesFromConnection().ConfigureAwait(false);
            }
            finally
            {
                Close();
            }
        });
    }

    private async Task CreateAndOpenConnection()
    {
        Connection = OnCreateConnection();

        await Connection.OpenAsync().ConfigureAwait(false);
        await ExecuteAuthCommandIfSettingsHasPassword().ConfigureAwait(false);

        if (Connected)
        {
            await ExecuteUpdatesCommands().ConfigureAwait(false);
            // gets an initial queue reading
            await ExecuteQueueInfoCommand().ConfigureAwait(false);
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected virtual FahClientConnection OnCreateConnection() => new(Settings!.Server, Settings.Port);

    private async Task ExecuteAuthCommandIfSettingsHasPassword()
    {
        if (!String.IsNullOrWhiteSpace(Settings?.Password))
        {
            await Connection!.CreateCommand("auth " + Settings.Password).ExecuteAsync().ConfigureAwait(false);
        }
    }

    private const int HeartbeatInterval = 60;

    private async Task ExecuteUpdatesCommands()
    {
        var heartbeatCommandText = String.Format(CultureInfo.InvariantCulture,
            "updates add 0 {0} $heartbeat", HeartbeatInterval);

        await Connection!.CreateCommand("updates clear").ExecuteAsync().ConfigureAwait(false);
        await Connection.CreateCommand("log-updates restart").ExecuteAsync().ConfigureAwait(false);
        await Connection.CreateCommand(heartbeatCommandText).ExecuteAsync().ConfigureAwait(false);
        await Connection.CreateCommand("updates add 1 1 $info").ExecuteAsync().ConfigureAwait(false);
        await Connection.CreateCommand("updates add 2 1 $(options -a)").ExecuteAsync().ConfigureAwait(false);
        await Connection.CreateCommand("updates add 3 1 $slot-info").ExecuteAsync().ConfigureAwait(false);
    }

    private async Task ExecuteQueueInfoCommand() =>
        await Connection!.CreateCommand("queue-info").ExecuteAsync().ConfigureAwait(false);

    private async Task ExecuteSlotOptionsCommand(int? slotId)
    {
        const string defaultSlotOptions = "slot-options {0} cpus client-type client-subtype cpu-usage machine-id max-packet-size core-priority next-unit-percentage max-units checkpoint pause-on-start gpu-index gpu-usage paused pci-bus pci-slot";
        var slotOptionsCommandText = String.Format(CultureInfo.InvariantCulture, defaultSlotOptions, slotId);
        await Connection!.CreateCommand(slotOptionsCommandText).ExecuteAsync().ConfigureAwait(false);
    }

    private async Task ReadMessagesFromConnection()
    {
        var reader = Connection!.CreateReader();
        try
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                await OnMessageRead(reader.Message).ConfigureAwait(false);
            }
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Debug(ex, Logger.NameFormat, Settings!.Name, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, Logger.NameFormat, Settings!.Name, ex.Message);
        }
    }

    private static readonly string[] _RefreshMessageTypes =
    {
        FahClientMessageType.Info,
        FahClientMessageType.Options,
        FahClientMessageType.SlotInfo,
        FahClientMessageType.SlotOptions,
        FahClientMessageType.QueueInfo,
        FahClientMessageType.LogRestart,
        FahClientMessageType.LogUpdate
    };

    protected virtual async Task OnMessageRead(FahClientMessage message)
    {
        _logger.Debug(Logger.NameFormat, Settings!.Name, $"{message.Identifier} - Length: {message.MessageText.Length}");

        MessageBuffer.Add(message);
        if (MessageBuffer.ContainsAny(_RefreshMessageTypes))
        {
            // TODO: Consider firing this Refresh as a new Task
            await Refresh().ConfigureAwait(false);
        }
    }
}

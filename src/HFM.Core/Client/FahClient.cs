using System.Globalization;

using HFM.Client;
using HFM.Core.Logging;

namespace HFM.Core.Client;

public class FahClient : Client
{
    protected FahClientConnection? Connection { get; set; }

    protected FahClientMessageBuffer MessageBuffer { get; set; }

    public override bool Connected => Connection is { Connected: true };

    private readonly ILogger _logger;

    public FahClient(ILogger? logger)
    {
        _logger = logger ?? NullLogger.Instance;

        MessageBuffer = new FahClientMessageBuffer();
    }

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

        // TODO: Clear the MessageBuffer
    }

    protected FahClientMessages? Messages { get; set; }

    protected override async Task OnRefresh()
    {
        Messages = await MessageBuffer.Empty();
        if (IsHeartbeatOverdue(Messages.Heartbeat))
        {
            Close();
        }

        await OnProcessMessages().ConfigureAwait(false);
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

    private Task OnProcessMessages()
    {
        var messages = Messages;
        // TODO: process the messages
        return Task.CompletedTask;
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
        FahClientMessageType.QueueInfo
    };

    protected virtual async Task OnMessageRead(FahClientMessage message)
    {
        MessageBuffer.Add(message);

        if (MessageBuffer.ContainsAny(_RefreshMessageTypes))
        {
            await Refresh().ConfigureAwait(false);
        }
    }
}

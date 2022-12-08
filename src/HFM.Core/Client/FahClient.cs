using System.Globalization;

using HFM.Client;
using HFM.Core.Logging;

namespace HFM.Core.Client;

public class FahClient : Client
{
    protected FahClientConnection? Connection { get; set; }

    public override bool Connected => Connection is { Connected: true };

    private readonly ILogger _logger;

    public FahClient(ILogger? logger)
    {
        _logger = logger ?? NullLogger.Instance;
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
    }

    protected Task? ReadMessagesTask { get; private set; }

    protected override async Task OnConnect()
    {
        await CreateAndOpenConnection().ConfigureAwait(false);

        if (ReadMessagesTask is not null)
        {
            ReadMessagesTask.Dispose();
        }

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

    public const int HeartbeatInterval = 60;

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

    protected virtual Task OnMessageRead(FahClientMessage message) => Task.CompletedTask;
}

using HFM.Client;
using HFM.Core.Logging;
using HFM.Core.WorkUnits;
using HFM.Preferences;

namespace HFM.Core.Client.Mocks;

public class MockFahClient : FahClient
{
    public new MockFahClientConnection? Connection => (MockFahClientConnection?)base.Connection;

    public MockFahClient(ClientSettings settings, ILogger? logger = null) : base(logger, new InMemoryPreferencesProvider(), NullProteinService.Instance)
    {
        Settings = settings;
    }

    public int CloseCount { get; private set; }

    protected override void OnClose()
    {
        CloseCount++;
        base.OnClose();
    }

    public FahClientMessages? LastMessages { get; private set; }
    public IReadOnlyCollection<ClientResource>? LastResources { get; private set; }

    protected override async Task OnRefresh()
    {
        await base.OnRefresh();
        if (Messages is not null)
        {
            LastMessages = Messages;
        }
        if (Resources is not null)
        {
            LastResources = Resources;
        }
    }

    public new Task? ReadMessagesTask => base.ReadMessagesTask;

    protected override FahClientConnection OnCreateConnection()
    {
        var connection = new MockFahClientConnection().HasMessages(_hasMessages);
        if (_connectionCloseException is not null)
        {
            connection.CloseThrows(_connectionCloseException);
        }
        if (_messageReaderReadException is not null)
        {
            connection.ReaderThrows(_messageReaderReadException);
        }
        return connection;
    }

    private readonly List<FahClientMessage> _hasMessages = new();

    public MockFahClient HasMessages(params FahClientMessage[] messages)
    {
        _hasMessages.AddRange(messages);
        return this;
    }

    private Exception? _connectionCloseException;

    public MockFahClient ConnectionThrowsOnClose(Exception exception)
    {
        _connectionCloseException = exception;
        return this;
    }

    private Exception? _messageReaderReadException;

    public MockFahClient MessageReaderThrowsOnRead(Exception exception)
    {
        _messageReaderReadException = exception;
        return this;
    }
}

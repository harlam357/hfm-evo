using HFM.Client;
using HFM.Core.Logging;

namespace HFM.Core.Client.Mocks;

public class MockFahClient : FahClient
{
    public new MockFahClientConnection? Connection => (MockFahClientConnection?)base.Connection;

    public MockFahClient(ClientSettings settings, ILogger? logger = null) : base(logger)
    {
        Settings = settings;
    }

    private Exception? _closeException;

    public MockFahClient ThrowsOnClose(Exception exception)
    {
        _closeException = exception;
        return this;
    }

    public new Task? ReadMessagesTask => base.ReadMessagesTask;

    protected override FahClientConnection OnCreateConnection()
    {
        var connection = new MockFahClientConnection().HasMessages(_messages);
        if (_closeException is not null)
        {
            connection.CloseThrows(_closeException);
        }
        if (_refreshException is not null)
        {
            connection.ReaderThrows(_refreshException);
        }
        return connection;
    }

    private readonly List<FahClientMessage> _messages = new();

    public MockFahClient HasMessages(params FahClientMessage[] messages)
    {
        _messages.AddRange(messages);
        return this;
    }

    private Exception? _refreshException;

    public MockFahClient ThrowsOnRefresh(Exception exception)
    {
        _refreshException = exception;
        return this;
    }

    public ICollection<FahClientMessage> MessagesRead { get; } = new List<FahClientMessage>();

    protected override async Task OnMessageRead(FahClientMessage message)
    {
        MessagesRead.Add(message);

        await base.OnMessageRead(message);
    }
}

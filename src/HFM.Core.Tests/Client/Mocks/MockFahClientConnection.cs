using System.Diagnostics;

using HFM.Client;

namespace HFM.Core.Client.Mocks;

public class MockFahClientConnection : FahClientConnection
{
    private bool _connected;

    public override bool Connected => _connected;

    public MockFahClientConnection() : base("", ClientSettings.DefaultPort)
    {

    }

    public override void Open() => _connected = true;

    public override Task OpenAsync()
    {
        _connected = true;
        return Task.CompletedTask;
    }

    public override void Close()
    {
        _connected = false;
        if (_closeException is not null)
        {
            throw _closeException;
        }
    }

    private Exception? _closeException;

    public MockFahClientConnection CloseThrows(Exception exception)
    {
        _closeException = exception;
        return this;
    }

    public IList<MockFahClientCommand> Commands { get; } = new List<MockFahClientCommand>();

    protected override FahClientCommand OnCreateCommand()
    {
        var command = new MockFahClientCommand(this);
        Commands.Add(command);
        return command;
    }

    protected override FahClientReader OnCreateReader()
    {
        var reader = new MockFahClientReader(this).EnqueueMessages(_messages);
        if (_readerException is not null)
        {
            reader.Throws(_readerException);
        }
        return reader;
    }

    private readonly List<FahClientMessage> _messages = new();

    public MockFahClientConnection HasMessages(IEnumerable<FahClientMessage> messages)
    {
        _messages.AddRange(messages);
        return this;
    }

    private Exception? _readerException;

    public MockFahClientConnection ReaderThrows(Exception exception)
    {
        _readerException = exception;
        return this;
    }
}

[DebuggerDisplay("{CommandText}")]
public class MockFahClientCommand : FahClientCommand
{
    public MockFahClientCommand(FahClientConnection connection) : base(connection)
    {

    }

    public bool Executed { get; private set; }

    public override int Execute()
    {
        Executed = true;
        return 0;
    }

    public override Task<int> ExecuteAsync()
    {
        Executed = true;
        return Task.FromResult(0);
    }
}

public class MockFahClientReader : FahClientReader
{
    public MockFahClientReader(FahClientConnection connection) : base(connection)
    {

    }

    public override bool Read()
    {
        var read = _messages.Count != 0;
        if (read)
        {
            Message = _messages.Dequeue();
        }
        else if (_exception is not null)
        {
            throw _exception;
        }

        return read;
    }

    public override Task<bool> ReadAsync()
    {
        var read = _messages.Count != 0;
        if (read)
        {
            Message = _messages.Dequeue();
        }
        else if (_exception is not null)
        {
            throw _exception;
        }

        return Task.FromResult(read);
    }

    private readonly Queue<FahClientMessage> _messages = new();

    public MockFahClientReader EnqueueMessages(IEnumerable<FahClientMessage> messages)
    {
        foreach (var m in messages)
        {
            _messages.Enqueue(m);
        }

        return this;
    }

    private Exception? _exception;

    public MockFahClientReader Throws(Exception exception)
    {
        _exception = exception;
        return this;
    }
}

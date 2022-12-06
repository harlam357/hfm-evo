namespace HFM.Core.Client;

public interface IClient
{
    event EventHandler<ClientChangedEventArgs>? ClientChanged;

    ClientSettings? Settings { get; }

    bool Connected { get; }

    void Close();

    Task Refresh();
}

internal interface ISetClientSettings
{
    void SetClientSettings(ClientSettings settings);
}

public abstract class Client : IClient, ISetClientSettings
{
    public event EventHandler<ClientChangedEventArgs>? ClientChanged;

    public ClientSettings? Settings { get; protected set; }

    public virtual bool Connected { get; protected set; }

    public void Close() => OnClose();

    protected virtual void OnClose() => Connected = false;

    public Task Refresh() => OnRefresh();

    protected virtual Task OnRefresh() => Task.CompletedTask;

    // ISetClientSettings
    public void SetClientSettings(ClientSettings settings) => Settings = settings;
}

public enum ClientChangedAction
{
    Invalidate
}

public sealed class ClientChangedEventArgs : EventArgs
{
    public ClientChangedAction Action { get; }

    public ClientChangedEventArgs(ClientChangedAction action)
    {
        Action = action;
    }
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullClient : Client, IDisposable
{
    public NullClient() : this(new ClientSettings { Guid = Guid.NewGuid() }) { }

    public NullClient(ClientSettings settings)
    {
        Settings = settings;
    }

    public void Dispose() { }
}

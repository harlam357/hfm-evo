namespace HFM.Core.Client;

public interface IClient
{
    event EventHandler<ClientChangedEventArgs>? ClientChanged;

    ClientSettings? Settings { get; }

    bool Connected { get; }

    void Close();
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
    public NullClient() : this(Guid.NewGuid()) { }

    public NullClient(Guid guid)
    {
        Settings = new ClientSettings { Guid = guid };
    }

    public void Dispose() { }
}

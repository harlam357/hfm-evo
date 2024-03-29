﻿namespace HFM.Core.Client;

public interface IClient
{
    event EventHandler<ClientChangedEventArgs>? ClientChanged;

    ClientIdentifier ClientIdentifier { get; }

    ClientSettings? Settings { get; }

    bool Connected { get; }

    void Close();

    Task Refresh(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the collection of client resources.
    /// </summary>
    IReadOnlyCollection<ClientResource>? Resources { get; }
}

internal interface ISetClientSettings
{
    void SetClientSettings(ClientSettings settings);
}

public abstract class Client : IClient, ISetClientSettings
{
    public event EventHandler<ClientChangedEventArgs>? ClientChanged;

    protected virtual void OnClientChanged(ClientChangedEventArgs args) =>
        ClientChanged?.Invoke(this, args);

    public ClientIdentifier ClientIdentifier => ClientIdentifier.FromSettings(Settings);

    public ClientSettings? Settings { get; protected set; }

    public virtual bool Connected { get; protected set; }

    public void Close() => OnClose();

    protected virtual void OnClose() => Connected = false;

    private int _refreshLock;

    // TODO: all methods called by Refresh() should accept a CancellationToken
    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        if (ClientCannotRefresh())
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _refreshLock, 1, 0) != 0)
        {
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Connected)
            {
                await OnConnect().ConfigureAwait(false);
            }

            await OnRefresh().ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Exchange(ref _refreshLock, 0);
        }
    }

    private bool ClientCannotRefresh() => Settings is { Disabled: true };

    protected virtual Task OnConnect()
    {
        Connected = true;
        return Task.CompletedTask;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected virtual Task OnRefresh() => Task.CompletedTask;

    // ISetClientSettings
    public void SetClientSettings(ClientSettings settings) => Settings = settings;

    private List<ClientResource>? _resources;

    public IReadOnlyCollection<ClientResource>? Resources => _resources;

    protected void SetResources(IEnumerable<ClientResource>? resources)
    {
        var newResources = resources is null ? null : new List<ClientResource>(resources);
        Interlocked.Exchange(ref _resources, newResources);
        OnClientChanged(new ClientChangedEventArgs(ClientChangedAction.Invalidate));
    }
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

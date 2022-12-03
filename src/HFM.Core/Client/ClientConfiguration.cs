using System.Collections;

namespace HFM.Core.Client;

public class ClientConfiguration : IEnumerable<IClient>
{
    public event EventHandler<ClientConfigurationChangedEventArgs>? ClientConfigurationChanged;

    protected virtual void OnClientConfigurationChanged(ClientConfigurationChangedEventArgs e) =>
        ClientConfigurationChanged?.Invoke(this, e);

    private void OnClientChanged(object? sender, ClientChangedEventArgs e)
    {
        if (e.Action is ClientChangedAction.Invalidate)
        {
            OnClientConfigurationChanged(
                new ClientConfigurationChangedEventArgs(
                    ClientConfigurationChangedAction.Invalidate,
                    sender as IClient));
        }
    }

    public bool IsDirty { get; set; }

    private readonly IClientFactory _clientFactory;
    private Dictionary<Guid, IClient> _clientsDictionary;

    public ClientConfiguration(IClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _clientsDictionary = new Dictionary<Guid, IClient>();
    }

    public void Load(IEnumerable<ClientSettings> settings)
    {
        Clear();

        var clients = new Dictionary<Guid, IClient>();
        foreach (var client in settings.Select(_clientFactory.Create))
        {
            if (client is not null)
            {
                clients.Add(client.Settings!.Guid, client);
                client.ClientChanged += OnClientChanged;
            }
        }

        if (clients.Count != 0)
        {
            Interlocked.Exchange(ref _clientsDictionary, clients);
            OnClientConfigurationChanged(new ClientConfigurationChangedEventArgs(ClientConfigurationChangedAction.Add, null));
        }
    }

    public void Add(ClientSettings settings)
    {
        var client = _clientFactory.Create(settings);
        if (client is null)
        {
            throw new ArgumentException("Given settings do not represent a valid client.", nameof(settings));
        }

        var clients = new Dictionary<Guid, IClient>(_clientsDictionary) { { settings.Guid, client } };
        client.ClientChanged += OnClientChanged;

        Interlocked.Exchange(ref _clientsDictionary, clients);

        IsDirty = true;
        OnClientConfigurationChanged(new ClientConfigurationChangedEventArgs(ClientConfigurationChangedAction.Add, client));
    }

    public void Edit(ClientSettings settings)
    {
        var editedClient = _clientFactory.Create(settings);
        if (editedClient is null)
        {
            throw new ArgumentException("Given settings do not represent a valid client.", nameof(settings));
        }

        var clients = new Dictionary<Guid, IClient>(_clientsDictionary);
        var client = clients[settings.Guid];

        clients.Remove(client.Settings!.Guid);
        KillClient(client);

        clients.Add(editedClient.Settings!.Guid, editedClient);
        editedClient.ClientChanged += OnClientChanged;

        Interlocked.Exchange(ref _clientsDictionary, clients);

        IsDirty = true;
        OnClientConfigurationChanged(new ClientConfigurationChangedEventArgs(ClientConfigurationChangedAction.Edit, client));
    }

    public void Remove(Guid clientGuid)
    {
        var clients = new Dictionary<Guid, IClient>(_clientsDictionary);
        var client = clients[clientGuid];

        clients.Remove(clientGuid);
        KillClient(client);

        Interlocked.Exchange(ref _clientsDictionary, clients);

        IsDirty = true;
        OnClientConfigurationChanged(new ClientConfigurationChangedEventArgs(ClientConfigurationChangedAction.Remove, client));
    }

    public void Clear()
    {
        var clients = new Dictionary<Guid, IClient>(_clientsDictionary);
        bool hasValues = clients.Count != 0;

        foreach (var client in clients.Values)
        {
            clients.Remove(client.Settings!.Guid);
            KillClient(client);
        }

        Interlocked.Exchange(ref _clientsDictionary, clients);

        IsDirty = false;
        if (hasValues)
        {
            OnClientConfigurationChanged(new ClientConfigurationChangedEventArgs(ClientConfigurationChangedAction.Clear, null));
        }
    }

    private void KillClient(IClient client)
    {
        client.ClientChanged -= OnClientChanged;
        client.Close();
        if (client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public int Count => _clientsDictionary.Count;

    public IEnumerator<IClient> GetEnumerator()
    {
        var clients = _clientsDictionary.Values.ToList();
        return clients.GetEnumerator();
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public enum ClientConfigurationChangedAction
{
    Add,
    Remove,
    Edit,
    Clear,
    Invalidate
}

public sealed class ClientConfigurationChangedEventArgs : EventArgs
{
    public ClientConfigurationChangedAction Action { get; }

    public IClient? Client { get; }

    public ClientConfigurationChangedEventArgs(ClientConfigurationChangedAction action, IClient? client)
    {
        Action = action;
        Client = client;
    }
}

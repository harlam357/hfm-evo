using HFM.Core.Net;

using Microsoft.Extensions.DependencyInjection;

namespace HFM.Core.Client;

public interface IClientFactory
{
    IClient? Create(ClientSettings settings);
}

public class ClientFactory : IClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDictionary<ClientType, Type> _typeMappings;

    public ClientFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _typeMappings = new Dictionary<ClientType, Type>
        {
            { ClientType.FahClient, typeof(FahClient) }
        };
    }

    public IClient? Create(ClientSettings settings)
    {
        // special consideration for obsolete ClientType values that may appear in hfmx configuration files
        if (!_typeMappings.TryGetValue(settings.ClientType, out var type))
        {
            return null;
        }

        ValidateClientSettings(settings);

        var client = (ISetClientSettings)_serviceProvider.GetRequiredService(type);
        client.SetClientSettings(settings);
        return (IClient)client;
    }

    private static void ValidateClientSettings(ClientSettings settings)
    {
        if (!ClientSettings.ValidateName(settings.Name))
        {
            throw new ArgumentException($"Client name {settings.Name} is not valid.", nameof(settings));
        }

        if (String.IsNullOrWhiteSpace(settings.Server))
        {
            throw new ArgumentException("Client server (host) name is empty.", nameof(settings));
        }

        if (!TcpPort.Validate(settings.Port))
        {
            throw new ArgumentException($"Client server (host) port {settings.Port} is not valid.", nameof(settings));
        }
    }
}

public sealed class NullClientFactory : IClientFactory
{
    public static NullClientFactory Instance { get; } = new();

    public IClient? Create(ClientSettings settings) => new NullClient(settings);
}

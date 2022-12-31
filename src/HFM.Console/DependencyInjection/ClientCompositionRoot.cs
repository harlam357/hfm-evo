using HFM.Core.Client;

using LightInject;

namespace HFM.Console.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class ClientCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton<ClientConfiguration>();
        serviceRegistry.AddSingleton<ClientScheduledTasks>();
        serviceRegistry.AddTransient<IClientFactory, ClientFactory>();
        serviceRegistry.AddTransient<FahClient>();
    }
}

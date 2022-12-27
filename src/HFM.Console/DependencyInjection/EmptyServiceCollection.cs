using Microsoft.Extensions.DependencyInjection;

namespace HFM.Console.DependencyInjection;

internal sealed class EmptyServiceCollection : List<ServiceDescriptor>, IServiceCollection
{
    internal static EmptyServiceCollection Instance { get; } = new();

    private EmptyServiceCollection()
    {

    }
}

using LightInject;

namespace HFM.Console.DependencyInjection;

internal static class LightInjectExtensions
{
    internal static IServiceRegistry AddSingleton<TService>(this IServiceRegistry container)
    {
        container.Register<TService>(new PerContainerLifetime());
        return container;
    }

    internal static IServiceRegistry AddSingleton<TService>(this IServiceRegistry container, Func<IServiceFactory, TService> factory)
    {
        container.Register(factory, new PerContainerLifetime());
        return container;
    }

    internal static IServiceRegistry AddSingleton<TService, TImplementation>(this IServiceRegistry container) where TImplementation : TService
    {
        container.Register<TService, TImplementation>(new PerContainerLifetime());
        return container;
    }

    internal static IServiceRegistry AddTransient<TService>(this IServiceRegistry container)
    {
        container.Register<TService>(new PerRequestLifeTime());
        return container;
    }

    internal static IServiceRegistry AddAddTransientSingleton<TService>(this IServiceRegistry container, Func<IServiceFactory, TService> factory)
    {
        container.Register(factory, new PerRequestLifeTime());
        return container;
    }

    internal static IServiceRegistry AddTransient<TService, TImplementation>(this IServiceRegistry container) where TImplementation : TService
    {
        container.Register<TService, TImplementation>(new PerRequestLifeTime());
        return container;
    }
}

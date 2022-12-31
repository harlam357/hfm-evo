using HFM.Preferences;

using LightInject;

namespace HFM.Console.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class PreferencesCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton<IPreferences>(_ => new XmlPreferencesProvider(Application.Path!, Application.DataFolderPath!, Application.Version));
    }
}

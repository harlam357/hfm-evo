using HFM.Preferences;

using LightInject;

namespace HFM.Console.DependencyInjection;

internal class PreferencesCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton<IPreferences>(_ => new XmlPreferencesProvider(Application.Path!, Application.DataFolderPath!, Application.Version));
    }
}

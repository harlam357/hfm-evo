using HFM.Core.Client;
using HFM.Preferences;

namespace HFM.Console.ViewModels;

internal class FahClientResourceViewModel : ClientResourceViewModel
{
    private readonly FahClientResource _clientResource;

    public FahClientResourceViewModel(FahClientResource clientResource, IPreferences preferences) : base(clientResource, preferences)
    {
        _clientResource = clientResource;
    }

    public override string Name =>
        _clientResource.SlotIdentifier.Name;
}

using HFM.Preferences.Data;

namespace HFM.Preferences;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class InMemoryPreferencesProvider : PreferencesProvider
{
    public InMemoryPreferencesProvider() : base(String.Empty, String.Empty, String.Empty)
    {

    }

    public InMemoryPreferencesProvider(PreferenceData data) : base(String.Empty, String.Empty, String.Empty, data)
    {

    }

    public InMemoryPreferencesProvider(string applicationPath, string applicationDataFolderPath, string applicationVersion)
        : base(applicationPath, applicationDataFolderPath, applicationVersion)
    {

    }
}

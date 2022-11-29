namespace HFM.Preferences;

public class InMemoryPreferencesProvider : PreferencesProvider
{
    public InMemoryPreferencesProvider() : base(String.Empty, String.Empty, String.Empty)
    {

    }

    public InMemoryPreferencesProvider(string applicationPath, string applicationDataFolderPath, string applicationVersion)
        : base(applicationPath, applicationDataFolderPath, applicationVersion)
    {

    }
}

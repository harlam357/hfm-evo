namespace HFM.Core.Client;

public interface IClient
{
    ClientSettings? Settings { get; }
}

internal interface ISetClientSettings
{
    void SetClientSettings(ClientSettings settings);
}

public abstract class Client : IClient, ISetClientSettings
{
    public ClientSettings? Settings { get; protected set; }

    // ISetClientSettings
    public void SetClientSettings(ClientSettings settings) => Settings = settings;
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullClient : Client
{

}

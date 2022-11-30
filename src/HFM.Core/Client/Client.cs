namespace HFM.Core.Client;

public interface IClient
{
    ClientSettings? Settings { get; }

    bool Connected { get; }

    void Close();
}

internal interface ISetClientSettings
{
    void SetClientSettings(ClientSettings settings);
}

public abstract class Client : IClient, ISetClientSettings
{
    public ClientSettings? Settings { get; protected set; }

    public virtual bool Connected { get; protected set; }

    public void Close() => OnClose();

    protected virtual void OnClose() => Connected = false;

    // ISetClientSettings
    public void SetClientSettings(ClientSettings settings) => Settings = settings;
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullClient : Client
{

}

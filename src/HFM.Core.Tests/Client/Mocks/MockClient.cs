namespace HFM.Core.Client.Mocks;

public class MockClient : Client
{
    public MockClient(ClientSettings settings)
    {
        Settings = settings;
    }

    public int RefreshCount { get; private set; }

    protected override Task OnRefresh()
    {
        RefreshCount++;
        return Task.CompletedTask;
    }
}

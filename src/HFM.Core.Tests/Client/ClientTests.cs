using HFM.Core.Client.Mocks;
using HFM.Core.Logging;

namespace HFM.Core.Client;

[TestFixture]
public class ClientTests
{
    private IClient? _client;

    public class GivenClientIsNotDisabled : ClientTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _client = new MockClient(new ClientSettings());

        [Test]
        public async Task RefreshConnectsTheClient()
        {
            await _client!.Refresh();
            Assert.That(_client.Connected, Is.True);
        }
    }

    public class GivenClientIsDisabled : ClientTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _client = new MockClient(new ClientSettings { Disabled = true });

        [Test]
        public async Task RefreshDoesNotConnectTheClient()
        {
            await _client!.Refresh();
            Assert.That(_client.Connected, Is.False);
        }
    }

    public class GivenRefreshIsCalledByMultipleThreads : ClientTests
    {
        private const int MaxThreads = 100;

        [SetUp]
        public virtual void BeforeEach()
        {
            _client = new MockClient(new ClientSettings());
            var tasks = Enumerable.Range(0, MaxThreads)
                .Select(_ => Task.Run(() => _client.Refresh()))
                .ToArray();
            Task.WaitAll(tasks);
        }

        [Test]
        public void RefreshDoesNotRunConcurrently()
        {
            var mockClient = (MockClient)_client!;
            TestLogger.Instance.Info($"Retrieve Count: {mockClient.RefreshCount}");
            Assert.That(mockClient.RefreshCount, Is.LessThanOrEqualTo(MaxThreads));
        }
    }
}

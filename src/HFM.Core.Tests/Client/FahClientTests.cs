using System.Text;

using HFM.Client;
using HFM.Core.Client.Mocks;
using HFM.Core.Logging;

using Moq;

namespace HFM.Core.Client;

[TestFixture]
public class FahClientTests
{
    private FahClient? _client;

    public class GivenClientIsNotDisabled : FahClientTests
    {
        [SetUp]
        public virtual void BeforeEach()
        {
            var mockClient = new MockFahClient(new ClientSettings())
                .HasMessages(
                    new FahClientMessage(
                        new FahClientMessageIdentifier(FahClientMessageType.Heartbeat, DateTime.Now),
                        new StringBuilder()));
            _client = mockClient;
        }

        [Test]
        public async Task RefreshConnectsTheClient()
        {
            await _client!.Refresh();
            Assert.That(_client.Connected, Is.True);
        }

        [Test]
        public async Task RefreshReadsMessages()
        {
            await _client!.Refresh();

            var mockClient = GetMockFahClient(_client!);
            await mockClient.ReadMessagesTask!;

            Assert.That(mockClient.MessagesRead, Has.Count.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task ThenThePreviousReaderTaskIsDisposed()
        {
            var mockClient = GetMockFahClient(_client!);

            await mockClient.Refresh();
            await mockClient.ReadMessagesTask!;

            Assert.DoesNotThrowAsync(async () => await mockClient.Refresh());
        }
    }

    public class GivenClientIsDisabled : FahClientTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _client = new MockFahClient(new ClientSettings { Disabled = true });

        [Test]
        public async Task RefreshDoesNotConnectTheClient()
        {
            await _client!.Refresh();
            Assert.That(_client.Connected, Is.False);
        }
    }

    public class GivenClientSettingsHasPassword : FahClientTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _client = new MockFahClient(new ClientSettings { Password = "foo" });

        [Test]
        public async Task RefreshExecutesAuthorizationCommand()
        {
            await _client!.Refresh();
            var connection = GetMockFahClientConnection(_client);
            Assert.That(connection.Commands.First().CommandText, Is.EqualTo("auth foo"));
        }
    }

    private class GivenMessageReaderThrowsObjectDisposedException : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .ThrowsOnRefresh(new ObjectDisposedException(""));
            _client = mockClient;
        }

        [Test]
        public async Task ThenDebugExceptionIsLogged()
        {
            await _client!.Refresh();

            var mockClient = GetMockFahClient(_client!);
            await mockClient.ReadMessagesTask!;

            var mockLogger = Mock.Get(_logger);
            mockLogger.Verify(x => x!.Log(LoggerLevel.Debug, It.IsAny<string>(), It.IsNotNull<Exception>()));
        }
    }

    private class GivenMessageReaderThrowsUnexpectedException : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .ThrowsOnRefresh(new InvalidOperationException(""));
            _client = mockClient;
        }

        [Test]
        public async Task ThenErrorExceptionIsLogged()
        {
            await _client!.Refresh();

            var mockClient = GetMockFahClient(_client!);
            await mockClient.ReadMessagesTask!;

            var mockLogger = Mock.Get(_logger);
            mockLogger.Verify(x => x!.Log(LoggerLevel.Error, It.IsAny<string>(), It.IsNotNull<Exception>()));
        }
    }

    private class GivenConnectionThrowsOnClose : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .ThrowsOnClose(new InvalidOperationException(""));
            _client = mockClient;
        }

        [Test]
        public async Task ThenErrorExceptionIsLogged()
        {
            await _client!.Refresh();
            _client.Close();

            var mockLogger = Mock.Get(_logger);
            mockLogger.Verify(x => x!.Log(LoggerLevel.Error, It.IsAny<string>(), It.IsNotNull<Exception>()));
        }
    }

    private static MockFahClient GetMockFahClient(FahClient client) => (MockFahClient)client;

    private static MockFahClientConnection GetMockFahClientConnection(FahClient client) =>
        GetMockFahClient(client).Connection!;
}

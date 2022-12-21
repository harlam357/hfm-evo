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

    [TestFixture]
    public class GivenClientMessages : FahClientTests
    {
        [SetUp]
        public virtual void BeforeEach()
        {
            var mockClient = new MockFahClient(new ClientSettings())
                .HasMessages(
                    FahClientMessageFileReader.ReadAllMessages(
                        Path.Combine(TestFiles.SolutionPath, "Client_v7_19")).ToArray());
            _client = mockClient;
        }

        [Test]
        public async Task RefreshReadsAllMessages()
        {
            await _client!.Refresh();

            var mockClient = GetMockFahClient(_client!);
            await mockClient.ReadMessagesTask!;

            var messages = mockClient.Messages!;
            Assert.Multiple(() =>
            {
                Assert.That(messages.ProcessedMessages, Has.Count.GreaterThanOrEqualTo(1));
                Assert.That(messages.Heartbeat, Is.Null);
                Assert.That(messages.Info, Is.Not.Null);
                Assert.That(messages.Options, Is.Not.Null);
                Assert.That(messages.SlotCollection, Has.Count.EqualTo(2));
                Assert.That(messages.UnitCollection, Has.Count.EqualTo(1));
            });
        }
    }

    [TestFixture]
    public class GivenClientIsNotDisabled : FahClientTests
    {
        [SetUp]
        public virtual void BeforeEach()
        {
            var mockClient = new MockFahClient(new ClientSettings())
                .HasMessages(
                    new FahClientMessage(
                        new FahClientMessageIdentifier(FahClientMessageType.Heartbeat, DateTime.UtcNow),
                        new StringBuilder()),
                    FahClientMessageFileReader.ReadMessage(
                        Path.Combine(TestFiles.SolutionPath, "Client_v7_19", "info-20210905T085332.txt")));
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

            Assert.That(mockClient.Messages!.ProcessedMessages, Has.Count.GreaterThanOrEqualTo(1));
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

    [TestFixture]
    public class GivenHeartbeatIsOverdue : FahClientTests
    {
        [SetUp]
        public virtual void BeforeEach()
        {
            var maximumMinutesBetweenHeartbeats = TimeSpan.FromMinutes(3);

            var mockClient = new MockFahClient(new ClientSettings())
                .HasMessages(
                    new FahClientMessage(
                        new FahClientMessageIdentifier(FahClientMessageType.Heartbeat, DateTime.UtcNow.Subtract(maximumMinutesBetweenHeartbeats)),
                        new StringBuilder()),
                    FahClientMessageFileReader.ReadMessage(
                        Path.Combine(TestFiles.SolutionPath, "Client_v7_19", "info-20210905T085332.txt")));
            _client = mockClient;
        }

        [Test]
        public async Task RefreshClosesTheClientConnection()
        {
            await _client!.Refresh();

            var mockClient = GetMockFahClient(_client!);
            await mockClient.ReadMessagesTask!;

            Assert.Multiple(() =>
            {
                Assert.That(_client.Connected, Is.False);
                // Close() called by OnRefresh() and completion of ReadMessagesTask
                Assert.That(mockClient.CloseCount, Is.EqualTo(2));
            });
        }
    }

    [TestFixture]
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

    [TestFixture]
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

    [TestFixture]
    public class GivenMessageReaderThrowsObjectDisposedExceptionOnRefresh : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .MessageReaderThrowsOnRead(new ObjectDisposedException(""));
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

    [TestFixture]
    public class GivenMessageReaderThrowsUnexpectedExceptionOnRefresh : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .MessageReaderThrowsOnRead(new InvalidOperationException(""));
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

    [TestFixture]
    public class GivenConnectionThrowsOnClose : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual void BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .ConnectionThrowsOnClose(new InvalidOperationException(""));
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

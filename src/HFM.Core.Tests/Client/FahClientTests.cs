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
        private MockFahClient? _mockClient;

        [SetUp]
        public virtual async Task BeforeEach()
        {
            _mockClient = new MockFahClient(new ClientSettings())
                .HasMessages(
                    FahClientMessageFileReader.ReadAllMessages(
                        Path.Combine(TestFiles.SolutionPath, "Client_v7_19")).ToArray());
            _client = _mockClient;
            await _client!.Refresh();
            await _mockClient.ReadMessagesTask!;
        }

        [Test]
        public void RefreshReadsAllMessages()
        {
            var messages = _mockClient!.LastMessages!;
            Assert.Multiple(() =>
            {
                Assert.That(messages.ProcessedMessages, Has.Count.GreaterThanOrEqualTo(1));
                Assert.That(messages.Heartbeat, Is.Null);
                Assert.That(messages.Info, Is.Not.Null);
                Assert.That(messages.Options, Is.Not.Null);
                Assert.That(messages.SlotCollection, Has.Count.EqualTo(2));
                Assert.That(messages.UnitCollection, Has.Count.EqualTo(1));
                Assert.That(messages.ClientRun, Is.Not.Null);
            });
        }

        [Test]
        public void RefreshCreatesClientResources()
        {
            var resources = _mockClient!.LastResources!.Cast<FahClientResource>().ToList();
            Assert.Multiple(() =>
            {
                Assert.That(resources, Has.Count.EqualTo(2));
                Assert.That(resources.All(x => x.Status != ClientResourceStatus.Unknown));
                Assert.That(resources.All(x => x.SlotId >= 0));
                Assert.That(resources.All(x => x.SlotDescription is not null));
                Assert.That(resources.All(x => x.LogLines!.Count > 0));
            });
        }

        [Test]
        public void RefreshSetsCpuSlotDescriptionProcessorToSystemCpu()
        {
            var resources = _mockClient!.LastResources!.Cast<FahClientResource>().ToList();
            Assert.Multiple(() =>
            {
                var cpuResource = resources.First();
                Assert.That(cpuResource.SlotDescription, Is.TypeOf<FahClientCpuSlotDescription>());
                var cpuSlotDescription = (FahClientCpuSlotDescription)cpuResource.SlotDescription!;
                Assert.That(cpuSlotDescription.Processor, Is.EqualTo("AMD Ryzen 7 3700X 8-Core Processor"));
            });
        }

        [Test]
        public void RefreshSendsSlotOptionsCommandAfterReceivingSlotInfo()
        {
            var messages = _mockClient!.LastMessages!;
            Assert.Multiple(() =>
            {
                Assert.That(messages.SlotCollection, Has.Count.EqualTo(2));
                var slotOptionsCommands = _mockClient.Connection!.Commands
                    .Where(x => x.CommandText!.StartsWith("slot-options", StringComparison.Ordinal))
                    .ToList();
                Assert.That(slotOptionsCommands, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void RefreshSendsQueueInfoCommandAfterReceivingLogUpdates()
        {
            var messages = _mockClient!.LastMessages!;
            Assert.Multiple(() =>
            {
                Assert.That(messages.ClientRun, Is.Not.Null);
                var queueInfoCommands = _mockClient.Connection!.Commands
                    .Where(x => x.CommandText == "queue-info")
                    .ToList();
                Assert.That(queueInfoCommands, Has.Count.EqualTo(4));
            });
        }

        [Test]
        public void MessagesAreClearedOnClose()
        {
            _client!.Close();
            Assert.That(_client!.Messages, Is.Null);
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
        private MockFahClient? _mockClient;

        [SetUp]
        public virtual async Task BeforeEach()
        {
            var maximumMinutesBetweenHeartbeats = TimeSpan.FromMinutes(3);

            _mockClient = new MockFahClient(new ClientSettings())
                .HasMessages(
                    new FahClientMessage(
                        new FahClientMessageIdentifier(FahClientMessageType.Heartbeat, DateTime.UtcNow.Subtract(maximumMinutesBetweenHeartbeats)),
                        new StringBuilder()),
                    FahClientMessageFileReader.ReadMessage(
                        Path.Combine(TestFiles.SolutionPath, "Client_v7_19", "info-20210905T085332.txt")));
            _client = _mockClient;
            await _client!.Refresh();
            await _mockClient.ReadMessagesTask!;
        }

        [Test]
        public void RefreshClosesTheClientConnection() =>
            Assert.Multiple(() =>
            {
                Assert.That(_client!.Connected, Is.False);
                // Close() called by OnRefresh() and completion of ReadMessagesTask
                Assert.That(_mockClient!.CloseCount, Is.EqualTo(2));
            });
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
        private MockFahClientConnection? _connection;

        [SetUp]
        public virtual async Task BeforeEach()
        {
            _client = new MockFahClient(new ClientSettings { Password = "foo" });
            await _client.Refresh();
            _connection = GetMockFahClientConnection(_client);
        }

        [Test]
        public void RefreshExecutesAuthorizationCommand() =>
            Assert.That(_connection!.Commands.First().CommandText, Is.EqualTo("auth foo"));
    }

    [TestFixture]
    public class GivenMessageReaderThrowsObjectDisposedExceptionOnRefresh : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual async Task BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .MessageReaderThrowsOnRead(new ObjectDisposedException(""));
            _client = mockClient;
            await _client!.Refresh();
            await mockClient.ReadMessagesTask!;
        }

        [Test]
        public void ThenDebugExceptionIsLogged()
        {
            var mockLogger = Mock.Get(_logger);
            mockLogger.Verify(x => x!.Log(LoggerLevel.Debug, It.IsAny<string>(), It.IsNotNull<Exception>()));
        }
    }

    [TestFixture]
    public class GivenMessageReaderThrowsUnexpectedExceptionOnRefresh : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual async Task BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .MessageReaderThrowsOnRead(new InvalidOperationException(""));
            _client = mockClient;
            await _client!.Refresh();
            await mockClient.ReadMessagesTask!;
        }

        [Test]
        public void ThenErrorExceptionIsLogged()
        {
            var mockLogger = Mock.Get(_logger);
            mockLogger.Verify(x => x!.Log(LoggerLevel.Error, It.IsAny<string>(), It.IsNotNull<Exception>()));
        }
    }

    [TestFixture]
    public class GivenConnectionThrowsOnClose : FahClientTests
    {
        private ILogger? _logger;

        [SetUp]
        public virtual async Task BeforeEach()
        {
            _logger = Mock.Of<ILogger>();
            var mockClient = new MockFahClient(new ClientSettings(), _logger)
                .ConnectionThrowsOnClose(new InvalidOperationException(""));
            _client = mockClient;
            await _client!.Refresh();
            _client.Close();
        }

        [Test]
        public void ThenErrorExceptionIsLogged()
        {
            var mockLogger = Mock.Get(_logger);
            mockLogger.Verify(x => x!.Log(LoggerLevel.Error, It.IsAny<string>(), It.IsNotNull<Exception>()));
        }
    }

    private static MockFahClient GetMockFahClient(FahClient client) => (MockFahClient)client;

    private static MockFahClientConnection GetMockFahClientConnection(FahClient client) =>
        GetMockFahClient(client).Connection!;
}

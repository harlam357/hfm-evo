using Moq;

namespace HFM.Core.Client;

[TestFixture]
public class ClientConfigurationTests
{
    private static ClientSettings CreateClientSettingsWithNewGuid() => new() { Guid = Guid.NewGuid() };

    private ClientConfiguration? _configuration;
    private ClientConfigurationChangedEventArgs? _configurationChangedEventArgs;

    [SetUp]
    public virtual void BeforeEach()
    {
        _configuration = new ClientConfiguration(NullClientFactory.Instance);
        _configuration.ClientConfigurationChanged += (_, args) => _configurationChangedEventArgs = args;
    }

    [TestFixture]
    public class WhenLoadingEmptySettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _configuration!.Load(Array.Empty<ClientSettings>());
        }

        [Test]
        public void ThenConfigurationContainsOneClient() =>
            Assert.That(_configuration, Has.Count.EqualTo(0));

        [Test]
        public void ThenConfigurationIsNotDirty() =>
            Assert.That(_configuration!.IsDirty, Is.False);

        [Test]
        public void ThenChangedEventIsNotRaised() =>
            Assert.That(_configurationChangedEventArgs, Is.Null);
    }

    [TestFixture]
    public class WhenLoadingSettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var settings = new[] { CreateClientSettingsWithNewGuid() };
            _configuration!.Load(settings);
        }

        [Test]
        public void ThenConfigurationContainsOneClient() =>
            Assert.That(_configuration, Has.Count.EqualTo(1));

        [Test]
        public void ThenConfigurationIsNotDirty() =>
            Assert.That(_configuration!.IsDirty, Is.False);

        [Test]
        public void ThenChangedEventIsRaised() =>
            Assert.Multiple(() =>
            {
                Assert.That(_configurationChangedEventArgs!.Action, Is.EqualTo(ClientConfigurationChangedAction.Add));
                Assert.That(_configurationChangedEventArgs!.Client, Is.Null);
            });

        [Test]
        public void ThenItEnumeratesTheClients()
        {
            using var enumerator = _configuration!.GetEnumerator();
            Assert.Multiple(() =>
            {
                Assert.That(enumerator.MoveNext(), Is.True);
                Assert.That(enumerator.Current, Is.Not.Null);
            });
        }
    }

    [TestFixture]
    public class WhenAddingSettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var settings = CreateClientSettingsWithNewGuid();
            _configuration!.Add(settings);
        }

        [Test]
        public void ThenConfigurationContainsOneClient() =>
            Assert.That(_configuration, Has.Count.EqualTo(1));

        [Test]
        public void ThenConfigurationIsDirty() =>
            Assert.That(_configuration!.IsDirty, Is.True);

        [Test]
        public void ThenChangedEventIsRaised() =>
            Assert.Multiple(() =>
            {
                Assert.That(_configurationChangedEventArgs!.Action, Is.EqualTo(ClientConfigurationChangedAction.Add));
                Assert.That(_configurationChangedEventArgs!.Client, Is.Not.Null);
            });
    }

    [TestFixture]
    public class WhenEditingSettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var settings = CreateClientSettingsWithNewGuid();
            _configuration!.Add(settings);
            settings = settings with { Name = "Foo" };
            _configuration.Edit(settings);
        }

        [Test]
        public void ThenConfigurationContainsOneClient() =>
            Assert.That(_configuration, Has.Count.EqualTo(1));

        [Test]
        public void ThenConfigurationIsDirty() =>
            Assert.That(_configuration!.IsDirty, Is.True);

        [Test]
        public void ThenChangedEventIsRaised() =>
            Assert.Multiple(() =>
            {
                Assert.That(_configurationChangedEventArgs!.Action, Is.EqualTo(ClientConfigurationChangedAction.Edit));
                Assert.That(_configurationChangedEventArgs!.Client, Is.Not.Null);
            });
    }

    [TestFixture]
    public class WhenRemovingSettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var settings = CreateClientSettingsWithNewGuid();
            _configuration!.Add(settings);
            _configuration.Remove(settings.Guid);
        }

        [Test]
        public void ThenConfigurationContainsZeroClients() =>
            Assert.That(_configuration, Has.Count.EqualTo(0));

        [Test]
        public void ThenConfigurationIsDirty() =>
            Assert.That(_configuration!.IsDirty, Is.True);

        [Test]
        public void ThenChangedEventIsRaised() =>
            Assert.Multiple(() =>
            {
                Assert.That(_configurationChangedEventArgs!.Action, Is.EqualTo(ClientConfigurationChangedAction.Remove));
                Assert.That(_configurationChangedEventArgs!.Client, Is.Not.Null);
            });
    }

    [TestFixture]
    public class WhenClearingSettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var settings = CreateClientSettingsWithNewGuid();
            _configuration!.Add(settings);
            _configuration.Clear();
        }

        [Test]
        public void ThenConfigurationContainsZeroClients() =>
            Assert.That(_configuration, Has.Count.EqualTo(0));

        [Test]
        public void ThenConfigurationIsNotDirty() =>
            Assert.That(_configuration!.IsDirty, Is.False);

        [Test]
        public void ThenChangedEventIsRaised() =>
            Assert.Multiple(() =>
            {
                Assert.That(_configurationChangedEventArgs!.Action, Is.EqualTo(ClientConfigurationChangedAction.Clear));
                Assert.That(_configurationChangedEventArgs!.Client, Is.Null);
            });
    }

    [TestFixture]
    public class WhenClearingEmptyConfiguration : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _configuration!.Clear();
        }

        [Test]
        public void ThenChangedEventIsNotRaised() =>
            Assert.That(_configurationChangedEventArgs, Is.Null);
    }

    [TestFixture]
    public class WhenClientChanges : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            var mockClient = new Mock<IClient>();

            _configuration = new ClientConfiguration(new GivenClientFactory(mockClient.Object));
            _configuration.ClientConfigurationChanged += (_, args) => _configurationChangedEventArgs = args;
            _configuration.Add(CreateClientSettingsWithNewGuid());

            mockClient.Raise(x => x.ClientChanged += null, this, new ClientChangedEventArgs(ClientChangedAction.Invalidate));
        }

        [Test]
        public void ThenChangedEventIsRaised() =>
            Assert.Multiple(() =>
            {
                Assert.That(_configurationChangedEventArgs!.Action, Is.EqualTo(ClientConfigurationChangedAction.Invalidate));
                Assert.That(_configurationChangedEventArgs!.Client, Is.Null);
            });
    }

    [TestFixture]
    public class WhenLoadingInvalidSettings : ClientConfigurationTests
    {
        [SetUp]
        public override void BeforeEach() =>
            _configuration = new ClientConfiguration(new GivenClientFactory(null));

        [Test]
        public void ThenAddingRaisesException() =>
            Assert.That(() => _configuration!.Add(new ClientSettings()), Throws.ArgumentException);

        [Test]
        public void ThenEditingRaisesException() =>
            Assert.That(() => _configuration!.Edit(new ClientSettings()), Throws.ArgumentException);
    }

    private sealed class GivenClientFactory : IClientFactory
    {
        private readonly IClient? _client;

        public GivenClientFactory(IClient? client)
        {
            _client = client;
        }

        public IClient? Create(ClientSettings settings) => _client;
    }
}

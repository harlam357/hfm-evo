namespace HFM.Core.Client;

[TestFixture]
public class FahClientSlotIdentifierTests
{
    [TestFixture]
    public class GivenTwoSlotIdentifiers : FahClientSlotIdentifierTests
    {
        private FahClientSlotIdentifier _x;
        private FahClientSlotIdentifier _y;

        [TestFixture]
        public class WhenClientGuidsAreEqual : GivenTwoSlotIdentifiers
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                var guid = Guid.NewGuid();
                _x = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, guid), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier("Fizz", "Bizz", 46330, guid), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsTrue() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y));
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreEqual() =>
                Assert.That(_x.GetHashCode(), Is.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareReturnsZero() =>
                Assert.That(_x.CompareTo(_y), Is.EqualTo(0));
        }

        [TestFixture]
        public class WhenClientGuidsAreNotEqual : GivenTwoSlotIdentifiers
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.NewGuid()), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.NewGuid()), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsFalse() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y), Is.False);
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreNotEqual() =>
                Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareReturnsNonZero() =>
                Assert.That(_x.CompareTo(_y), Is.Not.EqualTo(0));
        }

        [TestFixture]
        public class WhenFirstHasClientGuid : GivenTwoSlotIdentifiers
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.NewGuid()), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsFalse() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y), Is.False);
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreNotEqual() =>
                Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenFirstIsLessThanSecond() =>
                Assert.That(_x.CompareTo(_y), Is.EqualTo(-1));
        }

        [TestFixture]
        public class WhenSecondHasClientGuid : GivenTwoSlotIdentifiers
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.NewGuid()), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsFalse() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y), Is.False);
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreNotEqual() =>
                Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenFirstIsGreaterThanSecond() =>
                Assert.That(_x.CompareTo(_y), Is.EqualTo(1));
        }

        [TestFixture]
        public class WhenSecondIsNullObject : GivenTwoSlotIdentifiers
        {
            private new object? _y;

            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
                _y = null;
            }

            [Test]
            public void ThenEqualsReturnsFalse() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y), Is.False);
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenFirstIsGreaterThanSecond() =>
                Assert.That(_x.CompareTo(_y), Is.EqualTo(1));
        }

        [TestFixture]
        public class WhenSecondIsSlotIdentifierObject : GivenTwoSlotIdentifiers
        {
            private new object? _y;

            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsTrue() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y));
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreEqual() =>
                Assert.That(_x.GetHashCode(), Is.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareReturnsZero() =>
                Assert.That(_x.CompareTo(_y), Is.EqualTo(0));
        }

        [TestFixture]
        public class WhenClientNameServerAndPortAreEqual : GivenTwoSlotIdentifiers
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsTrue() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y));
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreEqual() =>
                Assert.That(_x.GetHashCode(), Is.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareReturnsZero() =>
                Assert.That(_x.CompareTo(_y), Is.EqualTo(0));
        }

        [TestFixture]
        public class WhenClientNameServerAndPortAreNotEqual : GivenTwoSlotIdentifiers
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _x = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
                _y = new FahClientSlotIdentifier(new ClientIdentifier("Fizz", "Bizz", 46330, Guid.Empty), FahClientSlotIdentifier.NoSlotId);
            }

            [Test]
            public void ThenEqualsReturnsFalse() =>
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
                Assert.That(_x.Equals(_y), Is.False);
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            [Test]
            public void ThenHashCodesAreNotEqual() =>
                Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareReturnsNonZero() =>
                Assert.That(_x.CompareTo(_y), Is.Not.EqualTo(0));
        }
    }

    [TestFixture]
    public class GivenSlotIdentifier : FahClientSlotIdentifierTests
    {
        private FahClientSlotIdentifier _identifier;

        [Test]
        public void WithServerAndPort()
        {
            _identifier = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty), 0);
            Assert.That(_identifier.ToString(), Is.EqualTo($"Foo Slot 00 (Bar:{ClientSettings.DefaultPort})"));
        }

        [Test]
        public void WithServer()
        {
            _identifier = new FahClientSlotIdentifier(new ClientIdentifier("Foo", "Bar", ClientSettings.NoPort, Guid.Empty), 0);
            Assert.That(_identifier.ToString(), Is.EqualTo("Foo Slot 00 (Bar)"));
        }

        [Test]
        public void WithOnlyName()
        {
            _identifier = new FahClientSlotIdentifier(new ClientIdentifier("Foo", null, ClientSettings.NoPort, Guid.Empty), 0);
            Assert.That(_identifier.ToString(), Is.EqualTo("Foo Slot 00"));
        }
    }

    [TestFixture]
    public class GivenSlotNameAndConnectionString : FahClientSlotIdentifierTests
    {
        private string? _name;
        private string? _connectionString;

        [Test]
        public void WithServerDashPortConnectionString()
        {
            _name = "Foo";
            _connectionString = "Server-12345";

            var identifier = FahClientSlotIdentifier.FromConnectionString(_name, _connectionString, Guid.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Name, Is.EqualTo(_name));
                Assert.That(identifier.SlotId, Is.EqualTo(-1));
                Assert.That(identifier.ClientIdentifier.Server, Is.EqualTo("Server"));
                Assert.That(identifier.ClientIdentifier.Port, Is.EqualTo(12345));
                Assert.That(identifier.ClientIdentifier.Guid, Is.EqualTo(Guid.Empty));
            });
        }

        [Test]
        public void WithServerColonPortConnectionString()
        {
            _name = "Foo";
            _connectionString = "Server:12345";

            var identifier = FahClientSlotIdentifier.FromConnectionString(_name, _connectionString, Guid.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Name, Is.EqualTo(_name));
                Assert.That(identifier.SlotId, Is.EqualTo(-1));
                Assert.That(identifier.HasSlotId, Is.False);
                Assert.That(identifier.ClientIdentifier.Server, Is.EqualTo("Server"));
                Assert.That(identifier.ClientIdentifier.Port, Is.EqualTo(12345));
                Assert.That(identifier.ClientIdentifier.Guid, Is.EqualTo(Guid.Empty));
            });
        }

        [Test]
        public void WithFileSystemPathConnectionString()
        {
            _name = "Bar";
            _connectionString = @"\\server\share";

            var identifier = FahClientSlotIdentifier.FromConnectionString(_name, _connectionString, Guid.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Name, Is.EqualTo(_name));
                Assert.That(identifier.SlotId, Is.EqualTo(-1));
                Assert.That(identifier.HasSlotId, Is.False);
                Assert.That(identifier.ClientIdentifier.Server, Is.EqualTo(_connectionString));
                Assert.That(identifier.ClientIdentifier.Port, Is.EqualTo(ClientSettings.NoPort));
                Assert.That(identifier.ClientIdentifier.Guid, Is.EqualTo(Guid.Empty));
            });
        }

        [Test]
        public void WithNameContainingSlotId()
        {
            _name = "Bar Slot 01";
            _connectionString = "Server:12345";

            var identifier = FahClientSlotIdentifier.FromConnectionString(_name, _connectionString, Guid.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Name, Is.EqualTo(_name));
                Assert.That(identifier.SlotId, Is.EqualTo(1));
                Assert.That(identifier.HasSlotId);
                Assert.That(identifier.ClientIdentifier.Server, Is.EqualTo("Server"));
                Assert.That(identifier.ClientIdentifier.Port, Is.EqualTo(12345));
                Assert.That(identifier.ClientIdentifier.Guid, Is.EqualTo(Guid.Empty));
            });
        }

        [Test]
        public void WithClientGuid()
        {
            _name = "Bar";
            _connectionString = @"\\server\share";

            var guid = Guid.NewGuid();
            var identifier = FahClientSlotIdentifier.FromConnectionString(_name, _connectionString, guid);

            Assert.Multiple(() =>
            {
                Assert.That(identifier.Name, Is.EqualTo(_name));
                Assert.That(identifier.SlotId, Is.EqualTo(-1));
                Assert.That(identifier.HasSlotId, Is.False);
                Assert.That(identifier.ClientIdentifier.Server, Is.EqualTo(_connectionString));
                Assert.That(identifier.ClientIdentifier.Port, Is.EqualTo(ClientSettings.NoPort));
                Assert.That(identifier.ClientIdentifier.Guid, Is.EqualTo(guid));
            });
        }
    }
}

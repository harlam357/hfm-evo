namespace HFM.Core.Client;

#pragma warning disable NUnit2010
#pragma warning disable NUnit2043

[TestFixture]
public class ClientIdentifierTests
{
    [TestFixture]
    public class GivenTwoClientIdentifiers : ClientIdentifierTests
    {
        private ClientIdentifier _x;
        private ClientIdentifier _y;

        [TestFixture]
        public class WhenGuidsAreEqual : GivenTwoClientIdentifiers
        {
            [SetUp]
            public void BeforeEach()
            {
                var guid = Guid.NewGuid();
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, guid);
                _y = new ClientIdentifier("Fizz", "Bizz", 46330, guid);
            }

            [Test]
            public void ThenEqualsReturnsTrue() => Assert.That(_x, Is.EqualTo(_y));

            [Test]
            public void ThenEqualsObjectReturnsTrue()
            {
                object obj = _y;
                Assert.That(_x.Equals(obj), Is.True);
            }

            [Test]
            public void ThenGetHashCodeReturnsTheSameValue() => Assert.That(_x.GetHashCode(), Is.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareToReturnsZero() => Assert.That(_x.CompareTo(_y), Is.EqualTo(0));

            [Test]
            public void ThenCompareToObjectReturnsZero()
            {
                object obj = _y;
                Assert.That(_x.CompareTo(obj), Is.EqualTo(0));
            }

            [Test]
            public void ThenEqualOperatorToReturnsTrue() => Assert.That(_x == _y, Is.True);

            [Test]
            public void ThenNotEqualOperatorToReturnsFalse() => Assert.That(_x != _y, Is.False);

            [Test]
            public void ThenLessThanOrEqualOperatorToReturnsTrue() => Assert.That(_x <= _y, Is.True);

            [Test]
            public void ThenGreaterThanOrEqualOperatorToReturnsTrue() => Assert.That(_x >= _y, Is.True);

            [Test]
            public void ThenLessThanOperatorToReturnsFalse() => Assert.That(_x < _y, Is.False);

            [Test]
            public void ThenGreaterThanOperatorToReturnsFalse() => Assert.That(_x > _y, Is.False);
        }

        [TestFixture]
        public class WhenGuidsAreNotEqual : GivenTwoClientIdentifiers
        {
            [SetUp]
            public void BeforeEach()
            {
                var guid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, guid);
                guid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
                _y = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, guid);
            }

            [Test]
            public void ThenEqualsReturnsFalse() => Assert.That(_x, Is.Not.EqualTo(_y));

            [Test]
            public void ThenEqualsObjectReturnsFalse()
            {
                object obj = _y;
                Assert.That(_x.Equals(obj), Is.False);
            }

            [Test]
            public void ThenGetHashCodeReturnsDifferentValue() => Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareToReturnsNonZero() => Assert.That(_x.CompareTo(_y), Is.Not.EqualTo(0));

            [Test]
            public void ThenCompareToObjectReturnsNonZero()
            {
                object obj = _y;
                Assert.That(_x.CompareTo(obj), Is.Not.EqualTo(0));
            }

            [Test]
            public void ThenEqualOperatorToReturnsFalse() => Assert.That(_x == _y, Is.False);

            [Test]
            public void ThenNotEqualOperatorToReturnsTrue() => Assert.That(_x != _y, Is.True);

            [Test]
            public void ThenLessThanOrEqualOperatorToReturnsFalse() => Assert.That(_x < _y, Is.False);

            [Test]
            public void ThenGreaterThanOrEqualOperatorToReturnsTrue() => Assert.That(_x >= _y, Is.True);

            [Test]
            public void ThenLessThanOperatorToReturnsFalse() => Assert.That(_x < _y, Is.False);

            [Test]
            public void ThenGreaterThanOperatorToReturnsTrue() => Assert.That(_x > _y, Is.True);
        }

        [TestFixture]
        public class WhenFirstHasGuid : GivenTwoClientIdentifiers
        {
            [SetUp]
            public void BeforeEach()
            {
                _x = new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.NewGuid());
                _y = new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty);
            }

            [Test]
            public void ThenEqualsReturnsFalse() => Assert.That(_x, Is.Not.EqualTo(_y));

            [Test]
            public void ThenGetHashCodeReturnsDifferentValue() => Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareToReturnsNegative() => Assert.That(_x.CompareTo(_y), Is.LessThanOrEqualTo(-1));
        }

        [TestFixture]
        public class WhenSecondHasGuid : GivenTwoClientIdentifiers
        {
            [SetUp]
            public void BeforeEach()
            {
                _x = new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.Empty);
                _y = new ClientIdentifier(null, null, ClientSettings.NoPort, Guid.NewGuid());
            }

            [Test]
            public void ThenEqualsReturnsFalse() => Assert.That(_x, Is.Not.EqualTo(_y));

            [Test]
            public void ThenGetHashCodeReturnsDifferentValue() => Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareToReturnsPositive() => Assert.That(_x.CompareTo(_y), Is.GreaterThanOrEqualTo(1));
        }

        [TestFixture]
        public class WhenNameServerPortAreEqualWithNoGuid : GivenTwoClientIdentifiers
        {
            [SetUp]
            public void BeforeEach()
            {
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);
                _y = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);
            }

            [Test]
            public void ThenEqualsReturnsTrue() => Assert.That(_x, Is.EqualTo(_y));

            [Test]
            public void ThenEqualsObjectReturnsTrue()
            {
                object obj = _y;
                Assert.That(_x.Equals(obj), Is.True);
            }

            [Test]
            public void ThenGetHashCodeReturnsTheSameValue() => Assert.That(_x.GetHashCode(), Is.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareToReturnsZero() => Assert.That(_x.CompareTo(_y), Is.EqualTo(0));

            [Test]
            public void ThenCompareToObjectReturnsZero()
            {
                object obj = _y;
                Assert.That(_x.CompareTo(obj), Is.EqualTo(0));
            }

            [Test]
            public void ThenEqualOperatorToReturnsTrue() => Assert.That(_x == _y, Is.True);

            [Test]
            public void ThenNotEqualOperatorToReturnsFalse() => Assert.That(_x != _y, Is.False);

            [Test]
            public void ThenLessThanOrEqualOperatorToReturnsTrue() => Assert.That(_x <= _y, Is.True);

            [Test]
            public void ThenGreaterThanOrEqualOperatorToReturnsTrue() => Assert.That(_x >= _y, Is.True);

            [Test]
            public void ThenLessThanOperatorToReturnsFalse() => Assert.That(_x < _y, Is.False);

            [Test]
            public void ThenGreaterThanOperatorToReturnsFalse() => Assert.That(_x > _y, Is.False);
        }

        [TestFixture]
        public class WhenNameServerPortAreNotEqualWithNoGuid : GivenTwoClientIdentifiers
        {
            [SetUp]
            public void BeforeEach()
            {
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);
                _y = new ClientIdentifier("Fizz", "Bizz", 46330, Guid.Empty);
            }

            [Test]
            public void ThenEqualsReturnsFalse() => Assert.That(_x, Is.Not.EqualTo(_y));

            [Test]
            public void ThenEqualsObjectReturnsFalse()
            {
                object obj = _y;
                Assert.That(_x.Equals(obj), Is.False);
            }

            [Test]
            public void ThenGetHashCodeReturnsDifferentValue() => Assert.That(_x.GetHashCode(), Is.Not.EqualTo(_y.GetHashCode()));

            [Test]
            public void ThenCompareToReturnsNonZero() => Assert.That(_x.CompareTo(_y), Is.Not.EqualTo(0));

            [Test]
            public void ThenCompareToObjectReturnsNonZero()
            {
                object obj = _y;
                Assert.That(_x.CompareTo(obj), Is.Not.EqualTo(0));
            }

            [Test]
            public void ThenEqualOperatorToReturnsFalse() => Assert.That(_x == _y, Is.False);

            [Test]
            public void ThenNotEqualOperatorToReturnsTrue() => Assert.That(_x != _y, Is.True);

            [Test]
            public void ThenLessThanOrEqualOperatorToReturnsFalse() => Assert.That(_x < _y, Is.False);

            [Test]
            public void ThenGreaterThanOrEqualOperatorToReturnsTrue() => Assert.That(_x >= _y, Is.True);

            [Test]
            public void ThenLessThanOperatorToReturnsFalse() => Assert.That(_x < _y, Is.False);

            [Test]
            public void ThenGreaterThanOperatorToReturnsTrue() => Assert.That(_x > _y, Is.True);
        }

        [TestFixture]
        public class ComparisonHasPrecedence : GivenTwoClientIdentifiers
        {
            [Test]
            public void NameComparisonHasFirstPrecedence()
            {
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);
                _y = new ClientIdentifier("Fizz", "Bizz", 46330, Guid.Empty);

                Assert.That(String.Compare(_x.Name, _y.Name, StringComparison.Ordinal), Is.EqualTo(_x.CompareTo(_y)));
            }

            [Test]
            public void ServerComparisonHasSecondPrecedence()
            {
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);
                _y = new ClientIdentifier("Foo", "Bizz", 46330, Guid.Empty);

                Assert.That(String.Compare(_x.Server, _y.Server, StringComparison.Ordinal), Is.EqualTo(_x.CompareTo(_y)));
            }

            [Test]
            public void PortComparisonHasThirdPrecedence()
            {
                _x = new ClientIdentifier("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);
                _y = new ClientIdentifier("Foo", "Bar", 46330, Guid.Empty);

                Assert.That(_x.Port.CompareTo(_y.Port), Is.EqualTo(_x.CompareTo(_y)));
            }
        }
    }

    [TestFixture]
    public class GivenOneClientIdentifier
    {
        private ClientIdentifier _identifier;

        [TestFixture]
        public class WhenNameServerAndDefaultPort : GivenOneClientIdentifier
        {
            [SetUp]
            public void BeforeEach() => _identifier = new("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty);

            [Test]
            public void ThenToStringReturnsNameServerAndPort()
            {
                string expected = $"Foo (Bar:{ClientSettings.DefaultPort})";
                Assert.That(_identifier.ToString(), Is.EqualTo(expected));
            }

            [Test]
            public void ThenToConnectionStringReturnsServerAndPort()
            {
                string expected = $"Bar:{ClientSettings.DefaultPort}";
                Assert.That(_identifier.ToConnectionString(), Is.EqualTo(expected));
            }
        }

        [TestFixture]
        public class WhenNameServerAndNoPort : GivenOneClientIdentifier
        {
            [SetUp]
            public void BeforeEach() => _identifier = new("Foo", "Bar", ClientSettings.NoPort, Guid.Empty);

            [Test]
            public void ThenToStringReturnsNameAndServer()
            {
                const string expected = "Foo (Bar)";
                Assert.That(_identifier.ToString(), Is.EqualTo(expected));
            }

            [Test]
            public void ThenToConnectionStringReturnsServer()
            {
                const string expected = "Bar";
                Assert.That(_identifier.ToConnectionString(), Is.EqualTo(expected));
            }
        }

        [TestFixture]
        public class WhenNameNullServerAndNoPort : GivenOneClientIdentifier
        {
            [SetUp]
            public void BeforeEach() => _identifier = new("Foo", null, ClientSettings.NoPort, Guid.Empty);

            [Test]
            public void ThenToStringReturnsName()
            {
                const string expected = "Foo";
                Assert.That(_identifier.ToString(), Is.EqualTo(expected));
            }

            [Test]
            public void ThenToConnectionStringReturnsNull() =>
                Assert.That(_identifier.ToConnectionString(), Is.Null);
        }

        [TestFixture]
        public class WhenComparedObjectIsNull : GivenOneClientIdentifier
        {
            [SetUp]
            public void BeforeEach() => _identifier = new("Foo", null, ClientSettings.NoPort, Guid.Empty);

            [Test]
            public void ThenEqualsObjectReturnsFalse()
            {
                object? obj = null;
                Assert.That(_identifier.Equals(obj), Is.False);
            }

            [Test]
            public void ThenCompareToObjectReturnsOne()
            {
                object? obj = null;
                Assert.That(_identifier.CompareTo(obj), Is.EqualTo(1));
            }
        }
    }

    [TestFixture]
    public class GivenConnectionString
    {
        private string? _connectionString;

        [TestFixture]
        public class WhenStringIsServerDashPort : GivenConnectionString
        {
            [SetUp]
            public void BeforeEach() => _connectionString = "Server-12345";

            [Test]
            public void ThenClientIdentifierIsParsed()
            {
                var identifier = ClientIdentifier.FromConnectionString("Foo", _connectionString, Guid.Empty);
                Assert.Multiple(() =>
                {
                    Assert.That(identifier.Name, Is.EqualTo("Foo"));
                    Assert.That(identifier.Server, Is.EqualTo("Server"));
                    Assert.That(identifier.Port, Is.EqualTo(12345));
                    Assert.That(identifier.Guid, Is.EqualTo(Guid.Empty));
                });
            }
        }

        [TestFixture]
        public class WhenStringIsServerColonPort : GivenConnectionString
        {
            [SetUp]
            public void BeforeEach() => _connectionString = "Server:12345";

            [Test]
            public void ThenClientIdentifierIsParsed()
            {
                var identifier = ClientIdentifier.FromConnectionString("Foo", _connectionString, Guid.Empty);
                Assert.Multiple(() =>
                {
                    Assert.That(identifier.Name, Is.EqualTo("Foo"));
                    Assert.That(identifier.Server, Is.EqualTo("Server"));
                    Assert.That(identifier.Port, Is.EqualTo(12345));
                    Assert.That(identifier.Guid, Is.EqualTo(Guid.Empty));
                });
            }
        }

        [TestFixture]
        public class WhenStringIsFileSystemPath : GivenConnectionString
        {
            [SetUp]
            public void BeforeEach() => _connectionString = @"\\server\share";

            [Test]
            public void ThenClientIdentifierIsParsed()
            {
                var guid = Guid.NewGuid();
                var identifier = ClientIdentifier.FromConnectionString("Foo", _connectionString, guid);
                Assert.Multiple(() =>
                {
                    Assert.That(identifier.Name, Is.EqualTo("Foo"));
                    Assert.That(identifier.Server, Is.EqualTo(_connectionString));
                    Assert.That(identifier.Port, Is.EqualTo(ClientSettings.NoPort));
                    Assert.That(identifier.Guid, Is.EqualTo(guid));
                });
            }
        }
    }
}

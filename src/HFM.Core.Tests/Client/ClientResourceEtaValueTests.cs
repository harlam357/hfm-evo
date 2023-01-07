using System.Globalization;

namespace HFM.Core.Client;

#pragma warning disable NUnit2010
#pragma warning disable NUnit2043

[TestFixture]
public class ClientResourceEtaValueTests
{
    [TestFixture]
    public class GivenTwoClientResourceEtaValues : ClientResourceEtaValueTests
    {
        private ClientResourceEtaValue _x;
        private ClientResourceEtaValue _y;

        [TestFixture]
        public class WhenEtaTimeSpansAreEqual : GivenTwoClientResourceEtaValues
        {
            [SetUp]
            public void BeforeEach()
            {
                _x = new(TimeSpan.FromMinutes(1), null);
                _y = new(TimeSpan.FromMinutes(1), null);
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
        public class WhenEtaTimeSpansAreNotEqual : GivenTwoClientResourceEtaValues
        {
            [SetUp]
            public void BeforeEach()
            {
                _x = new(TimeSpan.FromMinutes(2), null);
                _y = new(TimeSpan.FromMinutes(1), null);
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
    }

    [TestFixture]
    public class GivenOneClientResourceEtaValue
    {
        private ClientResourceEtaValue _value;

        [TestFixture]
        public class WhenEtaDateIsNull : GivenOneClientResourceEtaValue
        {
            [SetUp]
            public virtual void BeforeEach() =>
                _value = new(TimeSpan.FromMinutes(1), null);

#pragma warning disable CA1305
            [Test]
            public void ThenToStringReturnsTimeSpanString() =>
                Assert.That(_value.ToString(), Is.EqualTo("00:01:00"));
#pragma warning restore CA1305

            [Test]
            public void ThenToStringWithFormatReturnsTimeSpanString() =>
                Assert.That(_value.ToString(CultureInfo.InvariantCulture), Is.EqualTo("00:01:00"));
        }

        [TestFixture]
        public class WhenEtaDateIsNotNull : GivenOneClientResourceEtaValue
        {
            [SetUp]
            public virtual void BeforeEach() =>
                _value = new(TimeSpan.FromMinutes(1), new DateTime(2023, 1, 7, 12, 1, 0, DateTimeKind.Utc));

#pragma warning disable CA1305
            [Test]
            public void ThenToStringReturnsLocalDateTimeString() =>
                Assert.That(_value.ToString(), Is.EqualTo("1/7/2023 6:01:00 AM"));
#pragma warning restore CA1305

            [Test]
            public void ThenToStringWithFormatReturnsLocalDateTimeString() =>
                Assert.That(_value.ToString(CultureInfo.InvariantCulture), Is.EqualTo("01/07/2023 06:01:00"));
        }
    }
}

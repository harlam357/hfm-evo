using HFM.Preferences.Data;

namespace HFM.Preferences;

public partial class InMemoryPreferencesProviderTests
{
    [TestFixture]
    public class WhenSetting : InMemoryPreferencesProviderTests
    {
        [Test]
        public void ItThrowsOnDataTypeMismatch() =>
            Assert.That(() => _preferences!.Set(Preference.ClientRetrievalTask, String.Empty), Throws.ArgumentException);

        [Test]
        public void ItThrowsOnReadOnlyPreference() =>
            Assert.That(() => _preferences!.Set(Preference.ApplicationPath, "A"), Throws.InvalidOperationException);
    }

    [TestFixture]
    public class WhenSettingValueType : InMemoryPreferencesProviderTests
    {
        [Test]
        public void NullIsConvertedToDefault()
        {
            _preferences!.Set(Preference.FormSplitterLocation, (object?)null);
            Assert.That(_data!.MainWindowState.SplitterLocation, Is.EqualTo(0));
        }

        [Test]
        public void StringIsConvertedToValue()
        {
            _preferences!.Set(Preference.FormSplitterLocation, "60");
            Assert.That(_data!.MainWindowState.SplitterLocation, Is.EqualTo(60));
        }

        [Test]
        public void ValueIsSet()
        {
            _preferences!.Set(Preference.FormSplitterLocation, 120);
            Assert.That(_data!.MainWindowState.SplitterLocation, Is.EqualTo(120));
        }
    }

    [TestFixture]
    public class WhenSettingString : InMemoryPreferencesProviderTests
    {
        [Test]
        public void NullIsSet()
        {
            _preferences!.Set(Preference.EmailReportingFromAddress, (string?)null);
            Assert.That(_data!.Email.FromAddress, Is.EqualTo(null));
        }

        [Test]
        public void StringIsSet()
        {
            _preferences!.Set(Preference.EmailReportingFromAddress, "someone@home.com");
            Assert.That(_data!.Email.FromAddress, Is.EqualTo("someone@home.com"));
        }
    }

    [TestFixture]
    public class WhenSettingEnumValue : InMemoryPreferencesProviderTests
    {
        [Test]
        public void EnumIsConvertedToString()
        {
            _preferences!.Set(Preference.BonusCalculation, BonusCalculation.Default);
            Assert.That(_data!.ApplicationSettings.BonusCalculation, Is.EqualTo(nameof(BonusCalculation.Default)));
        }

        [Test]
        public void EnumIsConvertedToInt32()
        {
            _preferences!.Set(Preference.MessageLevel, LoggerLevel.Debug);
            Assert.That(_data!.ApplicationSettings.MessageLevel, Is.EqualTo((int)LoggerLevel.Debug));
        }
    }

    [TestFixture]
    public class WhenSettingReferenceType : InMemoryPreferencesProviderTests
    {
        [Test]
        public void NullIsSet()
        {
            ClientRetrievalTask? task = null;
            _preferences!.Set(Preference.ClientRetrievalTask, task);
            Assert.That(_data!.ClientRetrievalTask, Is.EqualTo(null));
        }

        [Test]
        public void ObjectIsDeepCopied()
        {
            var task = new ClientRetrievalTask();
            _preferences!.Set(Preference.ClientRetrievalTask, task);
            Assert.That(_data!.ClientRetrievalTask, Is.Not.SameAs(task));
        }
    }

    [TestFixture]
    public class WhenSettingCollectionValue : InMemoryPreferencesProviderTests
    {
        [Test]
        public void NullIsSet()
        {
            _preferences!.Set(Preference.FormColumns, (List<string>?)null);
            Assert.That(_data!.MainWindowGrid.Columns, Is.EqualTo(null));
        }

        [Test]
        public void EnumerableIsCopied()
        {
            var enumerable = (IEnumerable<string>)new[] { "a", "b", "c" };
            _preferences!.Set(Preference.FormColumns, enumerable);
            Assert.Multiple(() =>
            {
                Assert.That(_data!.MainWindowGrid.Columns, Has.Count.EqualTo(3));
                Assert.That(_data!.MainWindowGrid.Columns, Is.Not.SameAs(enumerable));
            });
        }

        [Test]
        public void CollectionIsCopied()
        {
            var enumerable = (ICollection<string>)new[] { "a", "b", "c" };
            _preferences!.Set(Preference.FormColumns, enumerable);
            Assert.Multiple(() =>
            {
                Assert.That(_data!.MainWindowGrid.Columns, Has.Count.EqualTo(3));
                Assert.That(_data!.MainWindowGrid.Columns, Is.Not.SameAs(enumerable));
            });
        }
    }
}

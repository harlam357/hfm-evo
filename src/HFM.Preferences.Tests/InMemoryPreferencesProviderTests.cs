using HFM.Preferences.Data;

namespace HFM.Preferences;

[TestFixture]
public partial class InMemoryPreferencesProviderTests
{
    private PreferenceData? _data;
    private IPreferences? _preferences;

    [SetUp]
    public virtual void BeforeEach()
    {
        _data = new PreferenceData();
        _preferences = new InMemoryPreferencesProvider(_data);
    }

    [TestFixture]
    public class GivenInMemoryPreferencesProvider : InMemoryPreferencesProviderTests
    {
        [Test]
        public void RoundTripsEncryptedPreference()
        {
            const string expected = "fizzbizz";
            _preferences!.Set(Preference.WebGenPassword, expected);
            Assert.That(_preferences!.Get<string>(Preference.WebGenPassword), Is.EqualTo(expected));
        }

        [Test]
        public void RaisesPreferenceChanged()
        {
            object? sender = null;
            PreferenceChangedEventArgs? args = null;
            _preferences!.PreferenceChanged += (s, e) =>
            {
                sender = s;
                args = e;
            };

            _preferences.Set(Preference.ColorLogFile, false);

            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.SameAs(_preferences));
                Assert.That(args!.Preference, Is.EqualTo(Preference.ColorLogFile));
            });
        }
    }

    private enum LoggerLevel
    {
        Info = 4,
        Debug = 5
    }

    private enum FtpMode
    {
        Default
    }

    private enum BonusCalculation
    {
        Default,
        DownloadTime
    }
}

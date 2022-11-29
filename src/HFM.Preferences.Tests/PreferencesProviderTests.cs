using HFM.Core;
using HFM.Preferences.Data;

namespace HFM.Preferences;

[TestFixture]
public partial class PreferencesProviderTests : IDisposable
{
    private const string ApplicationVersion = "1.0.0.0";
    private const string NoApplicationVersion = "0.0.0.0";

    private ArtifactFolder? _artifacts;
    private IPreferences? _preferences;

    [SetUp]
    public virtual void BeforeEach() => _artifacts = new ArtifactFolder();

    [TearDown]
    public virtual void AfterEach() => _artifacts?.Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _artifacts?.Dispose();
    }

    [TestFixture]
    public class XmlPreferencesProviderConstructorArgumentValues : PreferencesProviderTests
    {
        private string? _applicationPath;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _applicationPath = Environment.CurrentDirectory;
            _preferences = new XmlPreferencesProvider(_applicationPath, _artifacts!.Path, ApplicationVersion);
        }

        [Test]
        public void AreAvailableAsProperties()
        {
            var xml = (XmlPreferencesProvider?)_preferences;
            Assert.Multiple(() =>
            {
                Assert.That(xml!.ApplicationPath, Is.EqualTo(_applicationPath));
                Assert.That(xml.ApplicationDataFolderPath, Is.EqualTo(_artifacts!.Path));
                Assert.That(xml.ApplicationVersion, Is.EqualTo(ApplicationVersion));
            });
        }

        [Test]
        public void AreAvailableAsPreferences() =>
            Assert.Multiple(() =>
            {
                Assert.That(_preferences!.Get<string>(Preference.ApplicationPath), Is.EqualTo(_applicationPath));
                Assert.That(_preferences.Get<string>(Preference.ApplicationDataFolderPath), Is.EqualTo(_artifacts!.Path));
                Assert.That(_preferences.Get<string>(Preference.ApplicationVersion), Is.EqualTo(ApplicationVersion));

                string cacheDirectory = Path.Combine(_artifacts.Path, "logcache");
                Assert.That(_preferences.Get<string>(Preference.CacheDirectory), Is.EqualTo(cacheDirectory));
            });
    }

    [TestFixture]
    public class GivenXmlPreferencesProvider : PreferencesProviderTests
    {
        private string? _configPath;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _preferences = new XmlPreferencesProvider(Environment.CurrentDirectory, _artifacts!.Path, ApplicationVersion);
            _configPath = Path.Combine(_artifacts.Path, "config.xml");
        }

        [TestFixture]
        public class WhenNoConfigFileExists : GivenXmlPreferencesProvider
        {
            [Test]
            public void IsTrue() => Assert.That(!File.Exists(_configPath), Is.True);

            [Test]
            public void ResetCreatesTheFileWithApplicationVersion()
            {
                _preferences!.Reset();

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(_configPath), Is.True);
                    Assert.That(_preferences.Get<string>(Preference.ApplicationVersion), Is.EqualTo(ApplicationVersion));
                });
            }

            [Test]
            public void LoadCreatesTheFileWithApplicationVersion()
            {
                _preferences!.Load();

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(_configPath), Is.True);
                    Assert.That(_preferences.Get<string>(Preference.ApplicationVersion), Is.EqualTo(ApplicationVersion));
                });
            }

            [Test]
            public void SaveCreatesTheFileWithApplicationVersion()
            {
                _preferences!.Save();

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(_configPath), Is.True);
                    Assert.That(_preferences.Get<string>(Preference.ApplicationVersion), Is.EqualTo(ApplicationVersion));
                });
            }
        }

        [TestFixture]
        public class WhenBadConfigFileExists : GivenXmlPreferencesProvider
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                _preferences = new XmlPreferencesProvider(Environment.CurrentDirectory, _artifacts!.Path, ApplicationVersion);
                _configPath = Path.Combine(_artifacts.Path, "config.xml");
                File.WriteAllText(_configPath, String.Empty);
            }

            [Test]
            public void LoadCreatesTheFileWithApplicationVersion()
            {
                _preferences!.Load();

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(_configPath), Is.True);
                    Assert.That(_preferences.Get<string>(Preference.ApplicationVersion), Is.EqualTo(ApplicationVersion));
                });
            }
        }

        [TestFixture]
        public class WhenConfigFileExists : GivenXmlPreferencesProvider
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                _preferences = new XmlPreferencesProvider(Environment.CurrentDirectory, _artifacts!.Path, ApplicationVersion);
                _configPath = Path.Combine(_artifacts.Path, "config.xml");

                var data = new PreferenceData
                {
                    ApplicationVersion = ApplicationVersion,
                    ApplicationSettings =
                    {
                        CacheFolder = "foo"
                    }
                };
                XmlPreferencesProvider.WriteConfigXml(_configPath, data);
            }

            [Test]
            public void LoadsFromConfigFile()
            {
                _preferences!.Load();

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(_configPath), Is.True);
                    Assert.That(_preferences.Get<string>(Preference.ApplicationVersion), Is.EqualTo(ApplicationVersion));
                    Assert.That(_preferences.Get<string>(Preference.CacheFolder), Is.EqualTo("foo"));
                });
            }
        }

        [TestFixture]
        public class LoadExecutesProjectDownloadUrlUpgrade : GivenXmlPreferencesProvider
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                var data = new PreferenceData
                {
                    ApplicationVersion = NoApplicationVersion,
                    ApplicationSettings =
                    {
                        ProjectDownloadUrl = null
                    }
                };
                _preferences = new DoesNotReadFromOrWriteToDiskXmlPreferencesProvider(ApplicationVersion, data);
            }

            [Test]
            public void ThenProjectDownloadUrlIsUpdated()
            {
                _preferences!.Load();

                string actual = _preferences!.Get<string>(Preference.ProjectDownloadUrl);
                const string expected = "https://apps.foldingathome.org/psummary.json";
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [TestFixture]
        public class LoadExecutesMainWindowGridColumnsUpgrade : GivenXmlPreferencesProvider
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                var data = new PreferenceData
                {
                    ApplicationVersion = NoApplicationVersion,
                    MainWindowGrid =
                    {
                        Columns = new List<string>
                        {
                            "00,50,True,0",
                            "01,60,True,1",
                            "02,110,True,2",
                            "03,93,True,3",
                            "04,44,True,4"
                        }
                    }
                };
                _preferences = new DoesNotReadFromOrWriteToDiskXmlPreferencesProvider(ApplicationVersion, data);
            }

            [Test]
            public void ThenMainWindowGridColumnsAreUpdated()
            {
                _preferences!.Load();

                var actual = _preferences.Get<ICollection<string>>(Preference.FormColumns);
                var expected = new List<string>
                {
                    "00,50,True,0",
                    "01,60,True,1",
                    "02,110,True,2",
                    "03,93,True,3",
                    "05,44,True,5"
                };
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestFixture]
        public class LoadExecutesQueueSplitterLocationUpgrade : GivenXmlPreferencesProvider
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                var data = new PreferenceData
                {
                    ApplicationVersion = NoApplicationVersion,
                    MainWindowState =
                    {
                        QueueSplitterLocation = 0
                    }
                };
                _preferences = new DoesNotReadFromOrWriteToDiskXmlPreferencesProvider(ApplicationVersion, data);
            }

            [Test]
            public void ThenQueueSplitterLocationIsUpdated()
            {
                _preferences!.Load();

                int actual = _preferences!.Get<int>(Preference.QueueSplitterLocation);
                const int expected = 289;
                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }

    [TestFixture]
    public class GivenInMemoryPreferencesProvider : PreferencesProviderTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _preferences = new InMemoryPreferencesProvider(String.Empty, String.Empty, String.Empty);
        }

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

    private class DoesNotReadFromOrWriteToDiskXmlPreferencesProvider : XmlPreferencesProvider
    {
        private PreferenceData _data;

        public DoesNotReadFromOrWriteToDiskXmlPreferencesProvider(string applicationVersion, PreferenceData data)
            : base(Environment.CurrentDirectory, String.Empty, applicationVersion)
        {
            _data = data;
        }

        protected override PreferenceData OnRead() => _data;

        protected override void OnWrite(PreferenceData data) => _data = data;
    }

    private enum FtpMode
    {
        Default,
        Passive
    }

    private enum BonusCalculation
    {
        Default,
        DownloadTime
    }
}

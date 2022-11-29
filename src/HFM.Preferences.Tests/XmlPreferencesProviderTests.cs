using HFM.Core;
using HFM.Preferences.Data;

namespace HFM.Preferences;

[TestFixture]
public class XmlPreferencesProviderTests : IDisposable
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
    public class XmlPreferencesProviderConstructorArgumentValues : XmlPreferencesProviderTests
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
    public class GivenXmlPreferencesProvider : XmlPreferencesProviderTests
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
        public class LoadExecutesMainWindowGridColumnsInsertUpgrade : GivenXmlPreferencesProvider
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

        [TestFixture]
        public class LoadExecutesMainWindowGridColumnsDeleteUpgrade : GivenXmlPreferencesProvider
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                var data = new PreferenceData
                {
                    ApplicationVersion = "9.26.0.0",
                    MainWindowGrid =
                    {
                        Columns = new List<string>
                        {
                            "00,50,True,0",
                            "01,60,True,1",
                            "02,110,True,2",
                            "03,93,True,3",
                            "04,44,True,4",
                            "05,55,True,5",
                            "06,66,True,6",
                            "07,77,True,7",
                            "08,88,True,8",
                            "09,99,True,9",
                            "10,11,True,10"
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
                    "04,44,True,4",
                    "05,55,True,5",
                    "06,66,True,6",
                    "07,77,True,7",
                    "08,88,True,8",
                    "09,11,True,9"
                };
                CollectionAssert.AreEqual(expected, actual);
            }
        }
    }

    private sealed class DoesNotReadFromOrWriteToDiskXmlPreferencesProvider : XmlPreferencesProvider
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
}

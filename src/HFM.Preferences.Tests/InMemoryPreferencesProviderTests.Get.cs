using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

using HFM.Preferences.Data;

namespace HFM.Preferences;

public partial class InMemoryPreferencesProviderTests
{
    [TestFixture]
    public class WhenGetting : InMemoryPreferencesProviderTests
    {
        [Test]
        public void ValueType() =>
            Assert.That(_preferences!.Get<int>(Preference.FormSplitterLocation), Is.EqualTo(360));

        [Test]
        public void Int32AsEnum()
        {
            var data = new PreferenceData
            {
                ApplicationSettings =
                {
                    MessageLevel = 4
                }
            };
            _preferences = new InMemoryPreferencesProvider(data);
            Assert.That(_preferences!.Get<LoggerLevel>(Preference.MessageLevel), Is.EqualTo(LoggerLevel.Info));
        }

        [Test]
        public void NullStringItReturnsEmptyString() =>
            Assert.That(_preferences!.Get<string>(Preference.EmailReportingFromAddress), Is.EqualTo(String.Empty));

        [Test]
        public void NullStringAsEnumItReturnsEnumDefault()
        {
            var data = new PreferenceData
            {
                WebDeployment =
                {
                    FtpMode = null
                }
            };
            _preferences = new InMemoryPreferencesProvider(data);
            Assert.That(_preferences.Get<FtpMode>(Preference.WebGenFtpMode), Is.EqualTo(FtpMode.Default));
        }

        [Test]
        public void StringAsEnumItReturnsEnumDefaultWhenParsingFails()
        {
            var data = new PreferenceData
            {
                ApplicationSettings =
                {
                    BonusCalculation = "Foo"
                }
            };
            _preferences = new InMemoryPreferencesProvider(data);
            Assert.That(_preferences!.Get<BonusCalculation>(Preference.BonusCalculation), Is.EqualTo(BonusCalculation.Default));
        }

        [Test]
        public void StringAsEnum()
        {
            var data = new PreferenceData
            {
                ApplicationSettings =
                {
                    BonusCalculation = "DownloadTime"
                }
            };
            _preferences = new InMemoryPreferencesProvider(data);
            Assert.That(_preferences!.Get<BonusCalculation>(Preference.BonusCalculation), Is.EqualTo(BonusCalculation.DownloadTime));
        }

        [Test]
        public void StringValue() =>
            Assert.That(_preferences!.Get<string>(Preference.CacheFolder), Is.EqualTo("logcache"));

        [Test]
        public void ReferenceTypeItIsNewInstance()
        {
            var task1 = _preferences!.Get<ClientRetrievalTask>(Preference.ClientRetrievalTask);
            var task2 = _preferences.Get<ClientRetrievalTask>(Preference.ClientRetrievalTask);

            Assert.Multiple(() =>
            {
                Assert.That(task2, Is.Not.SameAs(task1));
                Assert.That(task1, Is.EqualTo(new ClientRetrievalTask()));
            });
        }

        [Test]
        public void NullCollection() =>
            Assert.That(_preferences!.Get<ICollection<string>>(Preference.FormColumns), Is.Null);

        [Test]
        public void CollectionValueItIsNewInstance()
        {
            var graphColors1 = _preferences!.Get<IEnumerable<Color>>(Preference.GraphColors);
            var graphColors2 = _preferences.Get<IEnumerable<Color>>(Preference.GraphColors);

            Assert.That(graphColors2, Is.Not.SameAs(graphColors1));
        }

        [Test]
        public void ItThrowsOnDataTypeMismatch() =>
            Assert.That(() => _preferences!.Get<int>(Preference.CacheFolder), Throws.ArgumentException);
    }

    [TestFixture]
    public class WhenBenchmarkingGet : InMemoryPreferencesProviderTests
    {
        private static void Benchmark(Action action, string name)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
            {
                action();
            }
            sw.Stop();
            Console.WriteLine("Get {0}: {1}ms", name, sw.ElapsedMilliseconds);
        }

        [Test]
        public void ValueType() => Benchmark(() => _preferences!.Get<int>(Preference.FormSplitterLocation), nameof(ValueType));

        [Test]
        public void NullString() => Benchmark(() => _preferences!.Get<string>(Preference.EmailReportingFromAddress), nameof(NullString));

        [Test]
        public void StringValue() => Benchmark(() => _preferences!.Get<string>(Preference.CacheFolder), nameof(StringValue));

        [Test]
        public void ReferenceType() => Benchmark(() => _preferences!.Get<ClientRetrievalTask>(Preference.ClientRetrievalTask), nameof(ReferenceType));

        [Test]
        public void NullCollection() => Benchmark(() => _preferences!.Get<ICollection<string>>(Preference.FormColumns), nameof(NullCollection));

        [Test]
        public void CollectionValue() => Benchmark(() => _preferences!.Get<IEnumerable<Color>>(Preference.GraphColors), nameof(CollectionValue));
    }

    [TestFixture]
    public class WhenGettingNestedValue : InMemoryPreferencesProviderTests
    {
        [Test]
        public void ItDefaultsParentPropertyValueWhenItIsNull()
        {
            var data = new PreferenceData
            {
                MainWindowGrid = null
            };
            _preferences = new InMemoryPreferencesProvider(data);

            Assert.Multiple(() =>
            {
                Assert.That(_preferences.Get<ICollection<string>>(Preference.FormColumns), Is.Null);
                Assert.That(_preferences.Get<ListSortDirection>(Preference.FormSortOrder), Is.EqualTo(ListSortDirection.Ascending));
                Assert.That(_preferences.Get<string>(Preference.FormSortColumn), Is.EqualTo(String.Empty));
            });
        }
    }
}

using HFM.Core.Client;
using HFM.Log;
using HFM.Preferences;

namespace HFM.Core.Artifacts.SlotMarkup;

[TestFixture]
public class SlotHtmlBuilderTests
{
    private const string CssFile = "Blue.css";

    private IPreferences? _preferences;
    private SlotHtmlBuilder? _builder;

    [SetUp]
    public virtual void BeforeEach()
    {
        _preferences = new InMemoryPreferencesProvider(TestFiles.ProjectPath, String.Empty, String.Empty);
        _preferences.Set(Preference.FormSortColumn, nameof(ClientResource.Core));
        _preferences.Set(Preference.CssFile, CssFile);
        _builder = new(_preferences);
    }

    [TestFixture]
    public class GivenClientResources : SlotHtmlBuilderTests
    {
        private List<ClientResource>? _resources;
        private SlotHtmlBuilderResult _result;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var utcNow = DateTime.UtcNow;

            _resources = new()
            {
                new()
                {
                    ClientIdentifier = new("Foo", null, ClientSettings.DefaultPort, Guid.Empty),
                    Status = ClientResourceStatus.Running,
                    WorkUnit = new()
                    {
                        UnitRetrievalTime = utcNow.Add(TimeSpan.FromMinutes(30)),
                        Assigned = utcNow,
                        FramesObserved = 4,
                        Frames = new Dictionary<int, LogLineFrameData>
                        {
                            { 0, new LogLineFrameData { ID = 0, Duration = TimeSpan.Zero } },
                            { 1, new LogLineFrameData { ID = 1, Duration = TimeSpan.FromMinutes(1) } },
                            { 2, new LogLineFrameData { ID = 2, Duration = TimeSpan.FromMinutes(2) } },
                            { 3, new LogLineFrameData { ID = 3, Duration = TimeSpan.FromMinutes(3) } }
                        },
                        Protein = new()
                        {
                            ProjectNumber = 1,
                            PreferredDays = 1,
                            MaximumDays = 3,
                            Credit = 1000,
                            Frames = 100,
                            Core = "0x22",
                            KFactor = 0.75
                        }
                    },
                    CompletedAndFailedWorkUnits = new()
                    {
                        RunCompleted = 20,
                        RunFailed = 2,
                        TotalCompleted = 200,
                        TotalFailed = 3
                    }
                },
                new()
                {
                    ClientIdentifier = new("Bar", null, ClientSettings.DefaultPort, Guid.Empty),
                    Status = ClientResourceStatus.Paused,
                    WorkUnit = new()
                    {
                        UnitRetrievalTime = utcNow.Add(TimeSpan.FromMinutes(60)),
                        Assigned = utcNow,
                        FramesObserved = 4,
                        Frames = new Dictionary<int, LogLineFrameData>
                        {
                            { 0, new LogLineFrameData { ID = 0, Duration = TimeSpan.Zero } },
                            { 1, new LogLineFrameData { ID = 1, Duration = TimeSpan.FromMinutes(4) } },
                            { 2, new LogLineFrameData { ID = 2, Duration = TimeSpan.FromMinutes(5) } },
                            { 3, new LogLineFrameData { ID = 3, Duration = TimeSpan.FromMinutes(6) } }
                        },
                        Protein = new()
                        {
                            ProjectNumber = 1,
                            PreferredDays = 1,
                            MaximumDays = 3,
                            Credit = 1000,
                            Frames = 100,
                            Core = "0x21",
                            KFactor = 0.75
                        }
                    },
                    CompletedAndFailedWorkUnits = new()
                    {
                        RunCompleted = 10,
                        RunFailed = 1,
                        TotalCompleted = 100,
                        TotalFailed = 2
                    }
                }
            };

            var xmlBuilder = new SlotXmlBuilder(_preferences!);
            var xmlResult = xmlBuilder.Build(_resources);
            _result = _builder!.Build(xmlResult);
        }

        [Test]
        public void ThenSlotHtmlContentIsReturned() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotSummaryHtml, Has.Count.EqualTo(2));
                Assert.That(_result.SlotDetailHtml, Has.Count.EqualTo(2));
            });

        [Test]
        public void ThenSlotHtmlContentIsExpectedLength() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotSummaryHtml.ElementAt(0).Content.Length, Is.GreaterThan(4800));
                Assert.That(_result.SlotSummaryHtml.ElementAt(1).Content.Length, Is.GreaterThan(4800));
                Assert.That(_result.SlotDetailHtml.ElementAt(0).Content.Length, Is.GreaterThan(5700));
                Assert.That(_result.SlotDetailHtml.ElementAt(1).Content.Length, Is.GreaterThan(5700));
            });

        [Test]
        public void ThenNamedContentIsReturned() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotSummaryHtml.ElementAt(0).Name, Is.EqualTo("index"));
                Assert.That(_result.SlotSummaryHtml.ElementAt(1).Name, Is.EqualTo("summary"));
                Assert.That(_result.SlotDetailHtml.ElementAt(0).Name, Is.EqualTo("Bar"));
                Assert.That(_result.SlotDetailHtml.ElementAt(1).Name, Is.EqualTo("Foo"));
            });

        [Test]
        public void ThenHtmlContentContainsSelectedStyleSheet() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotSummaryHtml.All(x => x.Content.ToString().Contains(CssFile)), Is.True);
                Assert.That(_result.SlotDetailHtml.All(x => x.Content.ToString().Contains(CssFile)), Is.True);
            });
    }
}

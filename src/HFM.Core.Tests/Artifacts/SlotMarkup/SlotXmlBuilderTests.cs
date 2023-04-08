using HFM.Core.Client;
using HFM.Log;
using HFM.Preferences;

namespace HFM.Core.Artifacts.SlotMarkup;

[TestFixture]
public class SlotXmlBuilderTests
{
    private SlotXmlBuilder? _builder;

    [SetUp]
    public virtual void BeforeEach()
    {
        var preferences = new InMemoryPreferencesProvider();
        preferences.Set(Preference.FormSortColumn, nameof(ClientResource.Core));
        _builder = new(preferences);
    }

    [TestFixture]
    public class GivenClientResources : SlotXmlBuilderTests
    {
        private List<ClientResource>? _resources;
        private SlotXmlBuilderResult _result;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            var utcNow = DateTime.UtcNow;

            _resources = new()
            {
                new()
                {
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

            _result = _builder!.Build(_resources!);
        }

        [Test]
        public void ThenSlotXmlModelsAreReturned() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotSummary, Is.Not.Null);
                Assert.That(_result.SlotSummary.Slots, Has.Count.EqualTo(2));
                Assert.That(_result.SlotDetails, Has.Count.EqualTo(2));
            });

        [Test]
        public void ThenSlotSummaryUpdateDateTimeIsSet() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotSummary.UpdateDateTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(_result.SlotSummary.UpdateDateTime.Kind, Is.EqualTo(DateTimeKind.Local));
            });

        [Test]
        public void ThenSlotSummaryHasXsltStyleNumberFormat() =>
            Assert.That(_result.SlotSummary.NumberFormat, Is.EqualTo("###,###,##0.0"));

        [Test]
        public void ThenSlotDetailUpdateDateTimeIsSet() =>
            Assert.Multiple(() =>
            {
                Assert.That(_result.SlotDetails.First().UpdateDateTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(_result.SlotDetails.First().UpdateDateTime.Kind, Is.EqualTo(DateTimeKind.Local));
            });

        [Test]
        public void ThenSlotDetailHasXsltStyleNumberFormat() =>
            Assert.That(_result.SlotDetails.First().NumberFormat, Is.EqualTo("###,###,##0.0"));
    }

    [TestFixture]
    public class GivenClientResourceWithDefaultProtein : SlotXmlBuilderTests
    {
        private List<ClientResource>? _resources;
        private SlotXmlBuilderResult _result;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();

            _resources = new()
            {
                new()
                {
                    WorkUnit = new()
                    {
                        Protein = new()
                    }
                }
            };

            _result = _builder!.Build(_resources!);
        }

        [Test]
        public void ThenSlotSummaryValuesMappedFromNullProteinValuesAreDefaultedToEmptyString() =>
            Assert.Multiple(() =>
            {
                var slotData = _result.SlotSummary.Slots![0];
                Assert.That(slotData.Core, Is.Empty);
                Assert.That(slotData.CoreId, Is.Empty);
                var protein = slotData.Protein!;
                Assert.That(protein.ServerIP, Is.Empty);
                Assert.That(protein.WorkUnitName, Is.Empty);
                Assert.That(protein.Core, Is.Empty);
                Assert.That(protein.Description, Is.Empty);
                Assert.That(protein.Contact, Is.Empty);
            });

        [Test]
        public void ThenSlotDetailValuesMappedFromNullProteinValuesAreDefaultedToEmptyString() =>
            Assert.Multiple(() =>
            {
                var slotData = _result.SlotDetails.First().SlotData!;
                Assert.That(slotData.Core, Is.Empty);
                Assert.That(slotData.CoreId, Is.Empty);
                var protein = slotData.Protein!;
                Assert.That(protein.ServerIP, Is.Empty);
                Assert.That(protein.WorkUnitName, Is.Empty);
                Assert.That(protein.Core, Is.Empty);
                Assert.That(protein.Description, Is.Empty);
                Assert.That(protein.Contact, Is.Empty);
            });
    }
}

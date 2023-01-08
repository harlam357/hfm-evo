using HFM.Log;

namespace HFM.Core.Client;

[TestFixture]
public class ClientResourceTotalsTests
{
    [TestFixture]
    public class GivenClientResourceCollectionIsNull : ClientResourceTotalsTests
    {
        [Test]
        public void ReturnsDefaultTotals()
        {
            var totals = ClientResourceTotals.Sum(null);
            Assert.That(totals, Is.EqualTo(default(ClientResourceTotals)));
        }
    }

    [TestFixture]
    public class GivenClientResourceCollectionHasResources : ClientResourceTotalsTests
    {
        private List<ClientResource>? _resources;

        [SetUp]
        public virtual void BeforeEach()
        {
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
        }

        [Test]
        public void ReturnsSummedTotals()
        {
            var totals = ClientResourceTotals.Sum(_resources);
            Assert.Multiple(() =>
            {
                Assert.That(totals.PPD, Is.EqualTo(15249.696));
                Assert.That(totals.UPD, Is.EqualTo(4.8));
                Assert.That(totals.TotalSlots, Is.EqualTo(2));
                Assert.That(totals.WorkingSlots, Is.EqualTo(1));
                Assert.That(totals.NonWorkingSlots, Is.EqualTo(1));
                Assert.That(totals.CompletedAndFailedWorkUnits.RunCompleted, Is.EqualTo(30));
                Assert.That(totals.CompletedAndFailedWorkUnits.RunFailed, Is.EqualTo(3));
                Assert.That(totals.CompletedAndFailedWorkUnits.TotalCompleted, Is.EqualTo(300));
                Assert.That(totals.CompletedAndFailedWorkUnits.TotalFailed, Is.EqualTo(5));
            });
        }
    }

}

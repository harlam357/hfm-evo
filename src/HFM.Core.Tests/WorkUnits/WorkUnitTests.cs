using HFM.Core.Collections;
using HFM.Log;
using HFM.Proteins;

namespace HFM.Core.WorkUnits;

[TestFixture]
public class WorkUnitTests
{
    private Protein? _protein;
    private WorkUnit? _workUnit;

    [SetUp]
    public virtual void BeforeEach() =>
        _protein = new Protein
        {
            ProjectNumber = 1,
            PreferredDays = 1,
            MaximumDays = 3,
            Credit = 100,
            Frames = 100,
            KFactor = 0.75
        };

    [TestFixture]
    public class GivenDefaultWorkUnit : WorkUnitTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();
            _workUnit = new WorkUnit();
        }

        [Test]
        public void ThenDefaultPropertyValuesAre() =>
            Assert.Multiple(() =>
            {
                Assert.That(_workUnit!.Id, Is.EqualTo(ItemIdentifier.NoId));
                Assert.That(_workUnit.UnitRetrievalTime, Is.EqualTo(DateTime.MinValue));
                Assert.That(_workUnit.DonorName, Is.Null);
                Assert.That(_workUnit.DonorTeam, Is.EqualTo(0));
                Assert.That(_workUnit.Assigned, Is.EqualTo(DateTime.MinValue));
                Assert.That(_workUnit.Timeout, Is.EqualTo(DateTime.MinValue));
                Assert.That(_workUnit.UnitStartTimeStamp, Is.EqualTo(TimeSpan.Zero));
                Assert.That(_workUnit.Finished, Is.Null);
                Assert.That(_workUnit.Core, Is.Null);
                Assert.That(_workUnit.CoreVersion, Is.Null);
                Assert.That(_workUnit.ProjectId, Is.EqualTo(0));
                Assert.That(_workUnit.ProjectRun, Is.EqualTo(0));
                Assert.That(_workUnit.ProjectClone, Is.EqualTo(0));
                Assert.That(_workUnit.ProjectGen, Is.EqualTo(0));
                Assert.That(_workUnit.UnitHex, Is.Null);
                Assert.That(_workUnit.Protein, Is.Null);
                Assert.That(_workUnit.Platform, Is.Null);
                Assert.That(_workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(_workUnit.LogLines, Is.Null);
                Assert.That(_workUnit.Frames, Is.Null);
                Assert.That(_workUnit.FramesObserved, Is.EqualTo(0));
                Assert.That(_workUnit.CurrentFrame, Is.Null);
                Assert.That(_workUnit.FramesComplete, Is.EqualTo(0));
                Assert.That(_workUnit.Progress, Is.EqualTo(0));
            });

        [TestCase(PpdCalculation.LastFrame)]
        [TestCase(PpdCalculation.LastThreeFrames)]
        [TestCase(PpdCalculation.AllFrames)]
        [TestCase(PpdCalculation.EffectiveRate)]
        public void ThenCalculateRawTime(PpdCalculation ppdCalculation) =>
            Assert.That(_workUnit!.GetRawTime(ppdCalculation), Is.EqualTo(0));

        [TestCase(PpdCalculation.LastFrame)]
        [TestCase(PpdCalculation.LastThreeFrames)]
        [TestCase(PpdCalculation.AllFrames)]
        [TestCase(PpdCalculation.EffectiveRate)]
        public void ThenCalculateFrameTime(PpdCalculation ppdCalculation) =>
            Assert.That(_workUnit!.GetFrameTime(ppdCalculation), Is.EqualTo(TimeSpan.FromSeconds(0)));

        [TestCase(PpdCalculation.LastFrame)]
        [TestCase(PpdCalculation.LastThreeFrames)]
        [TestCase(PpdCalculation.AllFrames)]
        [TestCase(PpdCalculation.EffectiveRate)]
        public void ThenCalculateCredit(PpdCalculation ppdCalculation) =>
            Assert.That(_workUnit!.GetCredit(ppdCalculation, BonusCalculation.None), Is.EqualTo(0.0));

        [TestCase(PpdCalculation.LastFrame)]
        [TestCase(PpdCalculation.LastThreeFrames)]
        [TestCase(PpdCalculation.AllFrames)]
        [TestCase(PpdCalculation.EffectiveRate)]
        public void ThenCalculatePpd(PpdCalculation ppdCalculation) =>
            Assert.That(_workUnit!.GetPpd(ppdCalculation, BonusCalculation.None), Is.EqualTo(0.0));
    }

    [TestFixture]
    public class GivenWorkUnitWithFrames : WorkUnitTests
    {
        private IReadOnlyDictionary<int, LogLineFrameData>? _frames;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();
            _frames = new Dictionary<int, LogLineFrameData>
            {
                { 0, new LogLineFrameData { ID = 0 } },
                { 1, new LogLineFrameData { ID = 1 } },
                { 5, new LogLineFrameData { ID = 5 } }
            };
            _workUnit = new WorkUnit
            {
                Frames = _frames
            };
        }

        [Test]
        public void ThenCurrentFrameIsSourcedFromFramesDictionary() =>
            Assert.That(_workUnit!.CurrentFrame, Is.SameAs(_frames![5]));

        [Test]
        public void ThenGetFrameReturnsNullWhenIdDoesNotExist() =>
            Assert.That(_workUnit!.GetFrame(2), Is.Null);

        [Test]
        public void ThenGetFrameReturnsObjectWhenIdExists() =>
            Assert.That(_workUnit!.GetFrame(0), Is.Not.Null);
    }

    [TestFixture]
    public class GivenWorkUnitWithNoFramesButFramesAreObserved : WorkUnitTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();
            _workUnit = new WorkUnit
            {
                FramesObserved = 4
            };
        }

        [TestCase(PpdCalculation.LastFrame)]
        [TestCase(PpdCalculation.LastThreeFrames)]
        [TestCase(PpdCalculation.AllFrames)]
        [TestCase(PpdCalculation.EffectiveRate)]
        public void ThenCalculateRawTime(PpdCalculation ppdCalculation) =>
            Assert.That(_workUnit!.GetRawTime(ppdCalculation), Is.EqualTo(0));
    }

    [TestFixture]
    public class GivenWorkUnitWithFourObservedFrames : WorkUnitTests
    {
        private Dictionary<int, LogLineFrameData>? _frames;

        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();
            _frames = new Dictionary<int, LogLineFrameData>
            {
                { 0, new LogLineFrameData { ID = 0, Duration = TimeSpan.Zero } },
                { 1, new LogLineFrameData { ID = 1, Duration = TimeSpan.FromMinutes(4) } },
                { 2, new LogLineFrameData { ID = 2, Duration = TimeSpan.FromMinutes(5) } },
                { 3, new LogLineFrameData { ID = 3, Duration = TimeSpan.FromMinutes(6) } }
            };
        }

        [TestFixture]
        public class WhenWorkUnitHasUnitRetrievalTime : GivenWorkUnitWithFourObservedFrames
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                var utcNow = DateTime.UtcNow;

                _workUnit = new WorkUnit
                {
                    UnitRetrievalTime = utcNow.Add(TimeSpan.FromMinutes(30)),
                    Assigned = utcNow,
                    FramesObserved = 4,
                    Frames = _frames,
                    Protein = _protein
                };
            }

            [Test]
            public void ThenCalculateRawTimeUsingEffectiveRate() =>
                Assert.That(_workUnit!.GetRawTime(PpdCalculation.EffectiveRate), Is.EqualTo(600));

            [Test]
            public void ThenCalculateFrameTimeUsingEffectiveRate() =>
                Assert.That(_workUnit!.GetFrameTime(PpdCalculation.EffectiveRate), Is.EqualTo(TimeSpan.FromSeconds(600)));

            [Test]
            public void ThenCalculatePpdUsingEffectiveRate() =>
                Assert.That(_workUnit!.GetPpd(PpdCalculation.EffectiveRate, BonusCalculation.None), Is.EqualTo(144.0));
        }

        [TestFixture]
        public class WhenWorkUnitHasFinishedTime : GivenWorkUnitWithFourObservedFrames
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _protein = new Protein
                {
                    ProjectNumber = 1,
                    PreferredDays = 3,
                    MaximumDays = 6,
                    Credit = 100,
                    Frames = 100,
                    KFactor = 5
                };

                var utcNow = DateTime.UtcNow;

                _workUnit = new WorkUnit
                {
                    Assigned = utcNow.Subtract(TimeSpan.FromHours(2)),
                    Finished = utcNow,
                    FramesObserved = 4,
                    Frames = _frames,
                    Protein = _protein
                };
            }

            [Test]
            public void ThenCalculateUpdUsingLastFrame() =>
                Assert.That(_workUnit!.GetUpd(PpdCalculation.LastFrame), Is.EqualTo(2.4));

            [Test]
            public void ThenCalculateCreditUsingLastFrameAndDownloadTime() =>
                Assert.That(_workUnit!.GetCredit(PpdCalculation.LastFrame, BonusCalculation.DownloadTime), Is.EqualTo(1897.367));

            [Test]
            public void ThenCalculatePpdUsingLastFrameAndDownloadTime() =>
                Assert.That(_workUnit!.GetPpd(PpdCalculation.LastFrame, BonusCalculation.DownloadTime), Is.EqualTo(4553.6808));

            [Test]
            public void ThenCalculateCreditUsingLastFrameAndFrameTime() =>
                Assert.That(_workUnit!.GetCredit(PpdCalculation.LastFrame, BonusCalculation.FrameTime), Is.EqualTo(848.528));

            [Test]
            public void ThenCalculatePpdUsingLastFrameAndFrameTime() =>
                Assert.That(_workUnit!.GetPpd(PpdCalculation.LastFrame, BonusCalculation.FrameTime), Is.EqualTo(2036.4672));

            [Test]
            public void ThenCalculateCreditUsingLastFrameAndNone() =>
                Assert.That(_workUnit!.GetCredit(PpdCalculation.LastFrame, BonusCalculation.None), Is.EqualTo(100.0));

            [Test]
            public void ThenCalculatePpdUsingLastFrameAndDownloadNone() =>
                Assert.That(_workUnit!.GetPpd(PpdCalculation.LastFrame, BonusCalculation.None), Is.EqualTo(240.0));

            [Test]
            public void ThenCalculateEtaUsingLastFrame() =>
                Assert.That(_workUnit!.GetEta(PpdCalculation.LastFrame), Is.EqualTo(TimeSpan.FromMinutes(582)));
        }

        [TestFixture]
        public class WhenWorkUnitHasNoFinishedTime : GivenWorkUnitWithFourObservedFrames
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _protein = new Protein
                {
                    ProjectNumber = 1,
                    PreferredDays = 3,
                    MaximumDays = 6,
                    Credit = 100,
                    Frames = 100,
                    KFactor = 5
                };

                var utcNow = DateTime.UtcNow;

                _workUnit = new WorkUnit
                {
                    UnitRetrievalTime = utcNow,
                    Assigned = utcNow.Subtract(TimeSpan.FromHours(2)),
                    FramesObserved = 4,
                    Frames = _frames,
                    Protein = _protein
                };
            }

            [Test]
            public void ThenCalculateCreditUsingLastFrameAndDownloadTime() =>
                Assert.That(_workUnit!.GetCredit(PpdCalculation.LastFrame, BonusCalculation.DownloadTime), Is.EqualTo(784.465));

            [Test]
            public void ThenCalculatePpdUsingLastFrameAndDownloadTime() =>
                Assert.That(_workUnit!.GetPpd(PpdCalculation.LastFrame, BonusCalculation.DownloadTime), Is.EqualTo(1882.716));
        }

        [TestFixture]
        public class WhenWorkUnitHasNoAssignedTime : GivenWorkUnitWithFourObservedFrames
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _protein = new Protein
                {
                    ProjectNumber = 1,
                    PreferredDays = 3,
                    MaximumDays = 6,
                    Credit = 100,
                    Frames = 100,
                    KFactor = 5
                };

                _workUnit = new WorkUnit
                {
                    FramesObserved = 4,
                    Frames = _frames,
                    Protein = _protein
                };
            }

            [Test]
            public void ThenCalculateCreditUsingLastFrameAndDownloadTime() =>
                Assert.That(_workUnit!.GetCredit(PpdCalculation.LastFrame, BonusCalculation.DownloadTime), Is.EqualTo(100.0));

            [Test]
            public void ThenCalculatePpdUsingLastFrameAndDownloadTime() =>
                Assert.That(_workUnit!.GetPpd(PpdCalculation.LastFrame, BonusCalculation.DownloadTime), Is.EqualTo(240.0));
        }
    }

    [TestFixture]
    public class GivenWorkUnitWithFiveObservedFrames : WorkUnitTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();
            var frames = new Dictionary<int, LogLineFrameData>
            {
                { 0, new LogLineFrameData { ID = 0, Duration = TimeSpan.Zero } },
                { 1, new LogLineFrameData { ID = 1, Duration = new TimeSpan(0, 5, 10) } },
                { 2, new LogLineFrameData { ID = 2, Duration = new TimeSpan(0, 6, 20) } },
                { 3, new LogLineFrameData { ID = 3, Duration = new TimeSpan(0, 6, 10) } },
                { 4, new LogLineFrameData { ID = 4, Duration = new TimeSpan(0, 6, 20) } }
            };

            _workUnit = new WorkUnit
            {
                FramesObserved = 5,
                Frames = frames,
                Protein = _protein
            };
        }

        [Test]
        public void ThenCalculateRawTimeUsingUnknownPpdCalculation() =>
            Assert.That(_workUnit!.GetRawTime((PpdCalculation)Int32.MaxValue), Is.EqualTo(0));

        [Test]
        public void ThenCalculateRawTimeUsingAllFrames() =>
            Assert.That(_workUnit!.GetRawTime(PpdCalculation.AllFrames), Is.EqualTo(360));

        [Test]
        public void ThenCalculateFrameTimeUsingAllFrames() =>
            Assert.That(_workUnit!.GetFrameTime(PpdCalculation.AllFrames), Is.EqualTo(TimeSpan.FromSeconds(360)));

        [Test]
        public void ThenCalculatePpdUsingAllFrames() =>
            Assert.That(_workUnit!.GetPpd(PpdCalculation.AllFrames, BonusCalculation.None), Is.EqualTo(240.0));

        [Test]
        public void ThenCalculateRawTimeUsingLastThreeFrames() =>
            Assert.That(_workUnit!.GetRawTime(PpdCalculation.LastThreeFrames), Is.EqualTo(376));

        [Test]
        public void ThenCalculateFrameTimeUsingLastThreeFrames() =>
            Assert.That(_workUnit!.GetFrameTime(PpdCalculation.LastThreeFrames), Is.EqualTo(TimeSpan.FromSeconds(376)));

        [Test]
        public void ThenCalculatePpdUsingLastThreeFrames() =>
            Assert.That(_workUnit!.GetPpd(PpdCalculation.LastThreeFrames, BonusCalculation.None), Is.EqualTo(229.78723));

        [Test]
        public void ThenCalculateRawTimeUsingLastFrame() =>
            Assert.That(_workUnit!.GetRawTime(PpdCalculation.LastFrame), Is.EqualTo(380));

        [Test]
        public void ThenCalculateFrameTimeUsingLastFrame() =>
            Assert.That(_workUnit!.GetFrameTime(PpdCalculation.LastFrame), Is.EqualTo(TimeSpan.FromSeconds(380)));

        [Test]
        public void ThenCalculatePpdUsingLastFrame() =>
            Assert.That(_workUnit!.GetPpd(PpdCalculation.LastFrame, BonusCalculation.None), Is.EqualTo(227.36842));
    }

    [TestFixture]
    public class GivenWorkUnitWithLastFrame : WorkUnitTests
    {
        [SetUp]
        public override void BeforeEach()
        {
            base.BeforeEach();
            var frames = new Dictionary<int, LogLineFrameData>
            {
                { 100, new LogLineFrameData { ID = 100, Duration = TimeSpan.Zero } }
            };

            _workUnit = new WorkUnit
            {
                Frames = frames,
                Protein = _protein
            };
        }

        [Test]
        public void ThenAllFramesAreCompleted() =>
            Assert.That(_workUnit!.AllFramesCompleted);
    }
}

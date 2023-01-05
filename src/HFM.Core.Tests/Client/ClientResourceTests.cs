using HFM.Core.WorkUnits;
using HFM.Log;
using HFM.Proteins;

namespace HFM.Core.Client;

[TestFixture]
public class ClientResourceTests
{
    private ClientResource? _resource;

    [TestFixture]
    public class GivenClientResourceHasRunningStatus : ClientResourceTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _resource = new ClientResource
            {
                Status = ClientResourceStatus.Running
            };

        [TestFixture]
        public class WhenWorkUnitIsNull : GivenClientResourceHasRunningStatus
        {
            [Test]
            public void ThenRunningStatusIsCalculated() =>
                Assert.That(_resource!.CalculateStatus(default), Is.EqualTo(ClientResourceStatus.Running));
        }

        [TestFixture]
        public class WhenWorkUnitHasProject : GivenClientResourceHasRunningStatus
        {
            [SetUp]
            public override void BeforeEach()
            {
                _resource = new FahClientResource
                {
                    SlotStatus = ClientResourceStatus.Running
                };
                _resource = _resource! with
                {
                    WorkUnit = new()
                    {
                        ProjectId = 1,
                        ProjectRun = 1,
                        ProjectClone = 1,
                        ProjectGen = 1
                    }
                };
            }

            [Test]
            public void ThenRunningNoFrameTimesStatusIsCalculated() =>
                Assert.That(_resource!.CalculateStatus(default), Is.EqualTo(ClientResourceStatus.RunningNoFrameTimes));

            [Test]
            public void ThenBonusCalculationByDownloadTimeIsNormalizedToFrameTime() =>
                Assert.That(_resource!.NormalizeBonusCalculation(default, BonusCalculation.DownloadTime), Is.EqualTo(BonusCalculation.FrameTime));

            [Test]
            public void ThenBonusCalculationNoneIsNormalizedToNone() =>
                Assert.That(_resource!.NormalizeBonusCalculation(default, BonusCalculation.None), Is.EqualTo(BonusCalculation.None));
        }

        [TestFixture]
        public class WhenWorkUnitHasFrames : GivenClientResourceHasRunningStatus
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _resource = _resource! with
                {
                    WorkUnit = new WorkUnit
                    {
                        Protein = new Protein
                        {
                            ProjectNumber = 1,
                            PreferredDays = 2,
                            MaximumDays = 3,
                            Credit = 1000.0,
                            Frames = 100,
                            KFactor = 0.75
                        },
                        Frames = new Dictionary<int, LogLineFrameData>
                        {
                            { 0, new LogLineFrameData { ID = 0, Duration = TimeSpan.Zero } },
                            { 1, new LogLineFrameData { ID = 1, Duration = TimeSpan.FromMinutes(1.1) } },
                            { 2, new LogLineFrameData { ID = 2, Duration = TimeSpan.FromMinutes(1) } },
                            { 3, new LogLineFrameData { ID = 3, Duration = TimeSpan.FromMinutes(0.9) } }
                        },
                        FramesObserved = 4
                    }
                };
            }

            [Test]
            public void ThenProgressIsCalculated() =>
                Assert.That(_resource!.CalculateProgress(default), Is.EqualTo(3));

            [Test]
            public void ThenFrameTimeIsCalculated() =>
                Assert.That(_resource!.CalculateFrameTime(PpdCalculation.LastThreeFrames), Is.EqualTo(TimeSpan.FromMinutes(1)));

            [Test]
            public void ThenPpdIsCalculated() =>
                Assert.That(_resource!.CalculatePointsPerDay(PpdCalculation.LastThreeFrames, BonusCalculation.FrameTime), Is.Not.EqualTo(0.0));

            [Test]
            public void ThenUpdIsCalculated() =>
                Assert.That(_resource!.CalculateUnitsPerDay(PpdCalculation.LastThreeFrames), Is.Not.EqualTo(0.0));

            [Test]
            public void ThenCreditIsCalculated() =>
                Assert.That(_resource!.CalculateCredit(PpdCalculation.LastThreeFrames, BonusCalculation.FrameTime), Is.Not.EqualTo(0.0));

            [Test]
            public void ThenEtaIsCalculated() =>
                Assert.That(_resource!.CalculateEta(PpdCalculation.LastThreeFrames), Is.Not.EqualTo(TimeSpan.Zero));

            [Test]
            public void ThenEtaDateIsCalculated() =>
                Assert.That(_resource!.CalculateEtaDate(PpdCalculation.LastThreeFrames), Is.Not.EqualTo(_resource.WorkUnit!.UnitRetrievalTime));
        }
    }

    [TestFixture]
    public class GivenClientResourceHasPausedStatus : ClientResourceTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _resource = new ClientResource
            {
                Status = ClientResourceStatus.Paused
            };

        [TestFixture]
        public class WhenWorkUnitHasFrames : GivenClientResourceHasPausedStatus
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _resource = _resource! with
                {
                    WorkUnit = new WorkUnit
                    {
                        Protein = new Protein
                        {
                            ProjectNumber = 1,
                            PreferredDays = 2,
                            MaximumDays = 3,
                            Credit = 1000.0,
                            Frames = 100,
                            KFactor = 0.75
                        },
                        Frames = new Dictionary<int, LogLineFrameData>
                        {
                            { 0, new LogLineFrameData { ID = 0, Duration = TimeSpan.Zero } },
                            { 1, new LogLineFrameData { ID = 1, Duration = TimeSpan.FromMinutes(1.1) } },
                            { 2, new LogLineFrameData { ID = 2, Duration = TimeSpan.FromMinutes(1) } },
                            { 3, new LogLineFrameData { ID = 3, Duration = TimeSpan.FromMinutes(0.9) } }
                        },
                        FramesObserved = 4
                    }
                };
            }

            [Test]
            public void ThenProgressIsCalculated() =>
                Assert.That(_resource!.CalculateProgress(default), Is.EqualTo(3));

            [Test]
            public void ThenZeroFrameTimeIsCalculated() =>
                Assert.That(_resource!.CalculateFrameTime(PpdCalculation.LastThreeFrames), Is.EqualTo(TimeSpan.Zero));

            [Test]
            public void ThenZeroPpdIsCalculated() =>
                Assert.That(_resource!.CalculatePointsPerDay(PpdCalculation.LastThreeFrames, BonusCalculation.FrameTime), Is.EqualTo(0.0));

            [Test]
            public void ThenZeroUpdIsCalculated() =>
                Assert.That(_resource!.CalculateUnitsPerDay(PpdCalculation.LastThreeFrames), Is.EqualTo(0.0));

            [Test]
            public void ThenBaseCreditIsCalculated() =>
                Assert.That(_resource!.CalculateCredit(PpdCalculation.LastThreeFrames, BonusCalculation.FrameTime), Is.EqualTo(1000.0));

            [Test]
            public void ThenZeroEtaIsCalculated() =>
                Assert.That(_resource!.CalculateEta(PpdCalculation.LastThreeFrames), Is.EqualTo(TimeSpan.Zero));

            [Test]
            public void ThenMinimumEtaDateIsCalculated() =>
                Assert.That(_resource!.CalculateEtaDate(PpdCalculation.LastThreeFrames), Is.EqualTo(DateTime.MinValue));
        }
    }

    [TestFixture]
    public class GivenClientResourceWithWorkUnitAndProtein : ClientResourceTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _resource = new ClientResource
            {
                WorkUnit = new WorkUnit
                {
                    Protein = new Protein
                    {
                        ProjectNumber = 1,
                        PreferredDays = 2,
                        MaximumDays = 3,
                        Credit = 1000.0,
                        Frames = 100,
                        KFactor = 0.75,
                        Core = "0x22"
                    },
                    CoreVersion = new Version(0, 0, 20)
                }
            };

        [Test]
        public void ThenCoreValueIsReturned() =>
            Assert.That(_resource!.FormatCore(false), Is.EqualTo("0x22"));

        [Test]
        public void ThenCoreValueAndVersionAreReturned() =>
            Assert.That(_resource!.FormatCore(true), Is.EqualTo("0x22 (0.0.20)"));
    }
}

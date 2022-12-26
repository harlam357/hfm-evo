using HFM.Core.Collections;
using HFM.Log;

namespace HFM.Core.WorkUnits;

[TestFixture]
public class WorkUnitTests
{
    private WorkUnit? _workUnit;

    [TestFixture]
    public class GivenDefaultWorkUnit : WorkUnitTests
    {
        [SetUp]
        public void BeforeEach() => _workUnit = new WorkUnit();

        [Test]
        public void ThenDefaultPropertyValuesAre() =>
            Assert.Multiple(() =>
            {
                Assert.That(_workUnit!.UnitRetrievalTime, Is.EqualTo(DateTime.MinValue));
                Assert.That(_workUnit.DonorName, Is.Null);
                Assert.That(_workUnit.DonorTeam, Is.EqualTo(0));
                Assert.That(_workUnit.Assigned, Is.EqualTo(DateTime.MinValue));
                Assert.That(_workUnit.Timeout, Is.EqualTo(DateTime.MinValue));
                Assert.That(_workUnit.UnitStartTimeStamp, Is.EqualTo(TimeSpan.Zero));
                Assert.That(_workUnit.Finished, Is.Null);
                Assert.That(_workUnit.CoreVersion, Is.Null);
                Assert.That(_workUnit.ProjectId, Is.EqualTo(0));
                Assert.That(_workUnit.ProjectRun, Is.EqualTo(0));
                Assert.That(_workUnit.ProjectClone, Is.EqualTo(0));
                Assert.That(_workUnit.ProjectGen, Is.EqualTo(0));
                Assert.That(_workUnit.Platform, Is.Null);
                Assert.That(_workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(_workUnit.FramesObserved, Is.EqualTo(0));
                Assert.That(_workUnit.CurrentFrame, Is.Null);
                Assert.That(_workUnit.LogLines, Is.Null);
                Assert.That(_workUnit.Frames, Is.Null);
                Assert.That(_workUnit.Core, Is.Null);
                Assert.That(_workUnit.UnitHex, Is.Null);
                Assert.That(_workUnit.Id, Is.EqualTo(ItemIdentifier.NoId));
            });
    }

    [TestFixture]
    public class GivenWorkUnitWithFrames : WorkUnitTests
    {
        private IReadOnlyDictionary<int, LogLineFrameData>? _frames;

        [SetUp]
        public void BeforeEach()
        {
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
}

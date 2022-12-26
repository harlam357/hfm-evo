using HFM.Client;
using HFM.Core.Client.Mocks;
using HFM.Core.WorkUnits;

namespace HFM.Core.Client;

[TestFixture]
public class FahClientWorkUnitCollectionBuilderTests
{
    private FahClientWorkUnitCollectionBuilder? _builder;
    private WorkUnitCollection? _workUnits;

    [TestFixture]
    public class GivenUnitCollectionIsNull : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public void BeforeEach()
        {
            _builder = new FahClientWorkUnitCollectionBuilder(new FahClientMessages());
            _workUnits = _builder!.BuildForSlot(0, new FahClientCpuSlotDescription(), null);
        }

        [Test]
        public void ThenBuildReturnsEmptyCollection() =>
            Assert.That(_workUnits, Has.Count.EqualTo(0));
    }

    [TestFixture]
    public class GivenClientOptionsIsNull : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_10");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                if (message.Identifier.MessageType == FahClientMessageType.Options)
                {
                    continue;
                }
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            _workUnits = _builder!.BuildForSlot(0, new FahClientCpuSlotDescription(), null);
        }

        [Test]
        public void ThenDonorNameAndTeamAreDefault()
        {
            var workUnit = _workUnits!.Current;
            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.DonorName, Is.EqualTo(Unknown.Value));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(0));
            });
        }
    }

    [TestFixture]
    public class GivenMessagesFromClientTenBuildsWorkUnitsForSlotZero : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_10");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            _workUnits = _builder!.BuildForSlot(0, new FahClientCpuSlotDescription(), null);
        }

        [Test]
        public void ThenOneWorkUnitIsReturned() =>
            Assert.That(_workUnits, Has.Count.EqualTo(1));

        [Test]
        public void ThenAllWorkUnitsHaveLogLines() =>
            Assert.That(_workUnits!.All(x => x.LogLines is null), Is.False);

        [Test]
        public void ThenCurrentIdIsSetForRunningWorkUnit() =>
            Assert.That(_workUnits!.CurrentId, Is.EqualTo(1));

        [Test]
        public void ThenCurrentWorkUnitIsPopulated()
        {
            var workUnit = _workUnits!.Current;

            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.UnitRetrievalTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(workUnit.DonorName, Is.EqualTo("harlam357"));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(32));
                Assert.That(workUnit.Assigned, Is.EqualTo(new DateTime(2012, 1, 10, 23, 20, 27)));
                Assert.That(workUnit.Timeout, Is.EqualTo(new DateTime(2012, 1, 22, 16, 22, 51)));
                Assert.That(workUnit.UnitStartTimeStamp, Is.EqualTo(new TimeSpan(3, 25, 32)));
                Assert.That(workUnit.Finished, Is.Null);
                Assert.That(workUnit.CoreVersion, Is.EqualTo(new Version(2, 27)));
                Assert.That(workUnit.ProjectId, Is.EqualTo(7610));
                Assert.That(workUnit.ProjectRun, Is.EqualTo(630));
                Assert.That(workUnit.ProjectClone, Is.EqualTo(0));
                Assert.That(workUnit.ProjectGen, Is.EqualTo(59));
                Assert.That(workUnit.Platform, Is.EqualTo(new WorkUnitPlatform(WorkUnitPlatformImplementation.CPU)));
                Assert.That(workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(workUnit.FramesObserved, Is.EqualTo(10));
                Assert.That(workUnit.CurrentFrame!.ID, Is.EqualTo(33));
                Assert.That(workUnit.CurrentFrame.RawFramesComplete, Is.EqualTo(660000));
                Assert.That(workUnit.CurrentFrame.RawFramesTotal, Is.EqualTo(2000000));
                Assert.That(workUnit.CurrentFrame.TimeStamp, Is.EqualTo(new TimeSpan(4, 46, 8)));
                Assert.That(workUnit.CurrentFrame.Duration, Is.EqualTo(new TimeSpan(0, 8, 31)));
                Assert.That(workUnit.LogLines, Has.Count.EqualTo(39));
                Assert.That(workUnit.Core, Is.EqualTo("0xa4"));
                Assert.That(workUnit.UnitHex, Is.EqualTo("0x00000050664f2dd04de6d4f93deb418d"));
            });
        }
    }

    [TestFixture]
    public class GivenClientRunIsNull : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_10");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                if (message.Identifier.MessageType == FahClientMessageType.LogRestart)
                {
                    continue;
                }
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            _workUnits = _builder!.BuildForSlot(0, new FahClientCpuSlotDescription(), null);
        }

        [Test]
        public void ThenOneWorkUnitIsReturned() =>
            Assert.That(_workUnits, Has.Count.EqualTo(1));

        [Test]
        public void ThenNoWorkUnitsHaveLogLines() =>
            Assert.That(_workUnits!.All(x => x.LogLines is null), Is.True);

        [Test]
        public void ThenCurrentIdIsSetForRunningWorkUnit() =>
            Assert.That(_workUnits!.CurrentId, Is.EqualTo(1));

        [Test]
        public void ThenCurrentWorkUnitIsPopulated()
        {
            var workUnit = _workUnits!.Current;

            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.UnitRetrievalTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(workUnit.DonorName, Is.EqualTo("harlam357"));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(32));
                Assert.That(workUnit.Assigned, Is.EqualTo(new DateTime(2012, 1, 10, 23, 20, 27)));
                Assert.That(workUnit.Timeout, Is.EqualTo(new DateTime(2012, 1, 22, 16, 22, 51)));
                Assert.That(workUnit.UnitStartTimeStamp, Is.EqualTo(TimeSpan.Zero));
                Assert.That(workUnit.Finished, Is.Null);
                Assert.That(workUnit.CoreVersion, Is.Null);
                Assert.That(workUnit.ProjectId, Is.EqualTo(7610));
                Assert.That(workUnit.ProjectRun, Is.EqualTo(630));
                Assert.That(workUnit.ProjectClone, Is.EqualTo(0));
                Assert.That(workUnit.ProjectGen, Is.EqualTo(59));
                Assert.That(workUnit.Platform, Is.EqualTo(new WorkUnitPlatform(WorkUnitPlatformImplementation.CPU)));
                Assert.That(workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(workUnit.FramesObserved, Is.EqualTo(0));
                Assert.That(workUnit.CurrentFrame, Is.Null);
                Assert.That(workUnit.LogLines, Is.Null);
                Assert.That(workUnit.Core, Is.EqualTo("0xa4"));
                Assert.That(workUnit.UnitHex, Is.EqualTo("0x00000050664f2dd04de6d4f93deb418d"));
            });
        }
    }

    [TestFixture]
    public class GivenMessagesFromClientTenBuildsWorkUnitsForSlotOne : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_10");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            _workUnits = _builder!.BuildForSlot(1, new FahClientCpuSlotDescription(), null);
        }

        [Test]
        public void ThenOneWorkUnitIsReturned() =>
            Assert.That(_workUnits, Has.Count.EqualTo(1));

        [Test]
        public void ThenAllWorkUnitsHaveLogLines() =>
            Assert.That(_workUnits!.All(x => x.LogLines is null), Is.False);

        [Test]
        public void ThenCurrentIdIsSetForRunningWorkUnit() =>
            Assert.That(_workUnits!.CurrentId, Is.EqualTo(2));

        [Test]
        public void ThenCurrentWorkUnitIsPopulated()
        {
            var workUnit = _workUnits!.Current;

            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.UnitRetrievalTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(workUnit.DonorName, Is.EqualTo("harlam357"));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(32));
                Assert.That(workUnit.Assigned, Is.EqualTo(new DateTime(2012, 1, 11, 4, 21, 14)));
                Assert.That(workUnit.Timeout, Is.EqualTo(DateTime.MinValue));
                Assert.That(workUnit.UnitStartTimeStamp, Is.EqualTo(new TimeSpan(4, 21, 52)));
                Assert.That(workUnit.Finished, Is.Null);
                Assert.That(workUnit.CoreVersion, Is.EqualTo(new Version(1, 31)));
                Assert.That(workUnit.ProjectId, Is.EqualTo(5772));
                Assert.That(workUnit.ProjectRun, Is.EqualTo(7));
                Assert.That(workUnit.ProjectClone, Is.EqualTo(364));
                Assert.That(workUnit.ProjectGen, Is.EqualTo(252));
                Assert.That(workUnit.Platform, Is.EqualTo(new WorkUnitPlatform(WorkUnitPlatformImplementation.CPU)));
                Assert.That(workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(workUnit.FramesObserved, Is.EqualTo(53));
                Assert.That(workUnit.CurrentFrame!.ID, Is.EqualTo(53));
                Assert.That(workUnit.CurrentFrame.RawFramesComplete, Is.EqualTo(53));
                Assert.That(workUnit.CurrentFrame.RawFramesTotal, Is.EqualTo(100));
                Assert.That(workUnit.CurrentFrame.TimeStamp, Is.EqualTo(new TimeSpan(4, 51, 53)));
                Assert.That(workUnit.CurrentFrame.Duration, Is.EqualTo(new TimeSpan(0, 0, 42)));
                Assert.That(workUnit.LogLines, Has.Count.EqualTo(98));
                Assert.That(workUnit.Core, Is.EqualTo("0x11"));
                Assert.That(workUnit.UnitHex, Is.EqualTo("0x241a68704f0d0e3a00fc016c0007168c"));
            });
        }
    }

    [TestFixture]
    public class GivenPreviousWorkUnitForSlotOne : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_10");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            var previousWorkUnit = new WorkUnit { Id = 0, ProjectId = 5767, ProjectRun = 3, ProjectClone = 138, ProjectGen = 144 };
            _workUnits = _builder!.BuildForSlot(1, new FahClientCpuSlotDescription(), previousWorkUnit);
        }

        [Test]
        public void ThenCurrentIdIsNotForPreviousWorkUnit() =>
            Assert.That(_workUnits!.CurrentId, Is.Not.EqualTo(0));

        [Test]
        public void ThenPreviousWorkUnitIsPopulated()
        {
            var workUnit = _workUnits![0];

            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.UnitRetrievalTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(workUnit.DonorName, Is.EqualTo(null));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(0));
                Assert.That(workUnit.Assigned, Is.EqualTo(DateTime.MinValue));
                Assert.That(workUnit.Timeout, Is.EqualTo(DateTime.MinValue));
                Assert.That(workUnit.UnitStartTimeStamp, Is.EqualTo(new TimeSpan(3, 25, 36)));
                Assert.That(workUnit.Finished, Is.Not.Null);
                Assert.That(workUnit.CoreVersion, Is.EqualTo(new Version(1, 31)));
                Assert.That(workUnit.ProjectId, Is.EqualTo(5767));
                Assert.That(workUnit.ProjectRun, Is.EqualTo(3));
                Assert.That(workUnit.ProjectClone, Is.EqualTo(138));
                Assert.That(workUnit.ProjectGen, Is.EqualTo(144));
                Assert.That(workUnit.Platform, Is.EqualTo(null));
                Assert.That(workUnit.Result, Is.EqualTo(WorkUnitResult.FinishedUnit));
                Assert.That(workUnit.FramesObserved, Is.EqualTo(100));
                Assert.That(workUnit.CurrentFrame!.ID, Is.EqualTo(100));
                Assert.That(workUnit.CurrentFrame.RawFramesComplete, Is.EqualTo(100));
                Assert.That(workUnit.CurrentFrame.RawFramesTotal, Is.EqualTo(100));
                Assert.That(workUnit.CurrentFrame.TimeStamp, Is.EqualTo(new TimeSpan(4, 21, 39)));
                Assert.That(workUnit.CurrentFrame.Duration, Is.EqualTo(new TimeSpan(0, 0, 33)));
                Assert.That(workUnit.LogLines, Has.Count.EqualTo(186));
                Assert.That(workUnit.Core, Is.EqualTo(null));
                Assert.That(workUnit.UnitHex, Is.EqualTo(null));
            });
        }
    }

    [TestFixture]
    public class GivenMessagesFromClientElevenBuildsWorkUnitsForSlotZero : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_11");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            _workUnits = _builder!.BuildForSlot(0, new FahClientCpuSlotDescription(), null);
        }

        [Test]
        public void ThenOneWorkUnitIsReturned() =>
            Assert.That(_workUnits, Has.Count.EqualTo(1));

        [Test]
        public void ThenAllWorkUnitsHaveLogLines() =>
            Assert.That(_workUnits!.All(x => x.LogLines is null), Is.False);

        [Test]
        public void ThenCurrentIdIsSetForRunningWorkUnit() =>
            Assert.That(_workUnits!.CurrentId, Is.EqualTo(1));

        [Test]
        public void ThenCurrentWorkUnitIsPopulated()
        {
            var workUnit = _workUnits!.Current;

            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.UnitRetrievalTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(workUnit.DonorName, Is.EqualTo("harlam357"));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(32));
                Assert.That(workUnit.Assigned, Is.EqualTo(new DateTime(2012, 2, 17, 21, 48, 22)));
                Assert.That(workUnit.Timeout, Is.EqualTo(new DateTime(2012, 2, 29, 14, 50, 46)));
                Assert.That(workUnit.UnitStartTimeStamp, Is.EqualTo(new TimeSpan(6, 34, 38)));
                Assert.That(workUnit.Finished, Is.Null);
                Assert.That(workUnit.CoreVersion, Is.EqualTo(new Version(2, 27)));
                Assert.That(workUnit.ProjectId, Is.EqualTo(7610));
                Assert.That(workUnit.ProjectRun, Is.EqualTo(192));
                Assert.That(workUnit.ProjectClone, Is.EqualTo(0));
                Assert.That(workUnit.ProjectGen, Is.EqualTo(58));
                Assert.That(workUnit.Platform, Is.EqualTo(new WorkUnitPlatform(WorkUnitPlatformImplementation.CPU)));
                Assert.That(workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(workUnit.FramesObserved, Is.EqualTo(3));
                Assert.That(workUnit.CurrentFrame!.ID, Is.EqualTo(95));
                Assert.That(workUnit.CurrentFrame.RawFramesComplete, Is.EqualTo(1900000));
                Assert.That(workUnit.CurrentFrame.RawFramesTotal, Is.EqualTo(2000000));
                Assert.That(workUnit.CurrentFrame.TimeStamp, Is.EqualTo(new TimeSpan(6, 46, 16)));
                Assert.That(workUnit.CurrentFrame.Duration, Is.EqualTo(new TimeSpan(0, 4, 50)));
                Assert.That(workUnit.LogLines, Has.Count.EqualTo(32));
                Assert.That(workUnit.Core, Is.EqualTo("0xa4"));
                Assert.That(workUnit.UnitHex, Is.EqualTo("0x0000004e664f2dd04de6d35869ac2ae3"));
            });
        }
    }

    [TestFixture]
    public class GivenMessagesFromClientNineteenBuildsWorkUnitsForSlotOne : FahClientWorkUnitCollectionBuilderTests
    {
        [SetUp]
        public async Task BeforeEach()
        {
            var buffer = new FahClientMessageBuffer();
            var path = Path.Combine(TestFiles.SolutionPath, "Client_v7_19");
            foreach (var message in FahClientMessageFileReader.ReadAllMessages(path))
            {
                buffer.Add(message);
            }
            _builder = new FahClientWorkUnitCollectionBuilder(await buffer.Empty());
            var slotDescription = new FahClientGpuSlotDescription { GpuBus = 13, GpuSlot = 0 };
            _workUnits = _builder!.BuildForSlot(1, slotDescription, null);
        }

        [Test]
        public void ThenOneWorkUnitIsReturned() =>
            Assert.That(_workUnits, Has.Count.EqualTo(1));

        [Test]
        public void ThenAllWorkUnitsHaveLogLines() =>
            Assert.That(_workUnits!.All(x => x.LogLines is null), Is.False);

        [Test]
        public void ThenCurrentIdIsSetForRunningWorkUnit() =>
            Assert.That(_workUnits!.CurrentId, Is.EqualTo(0));

        [Test]
        public void ThenCurrentWorkUnitIsPopulated()
        {
            var workUnit = _workUnits!.Current;

            Assert.Multiple(() =>
            {
                Assert.That(workUnit!.UnitRetrievalTime, Is.Not.EqualTo(default(DateTime)));
                Assert.That(workUnit.DonorName, Is.EqualTo("harlam357"));
                Assert.That(workUnit.DonorTeam, Is.EqualTo(32));
                Assert.That(workUnit.Assigned, Is.EqualTo(new DateTime(2021, 9, 5, 17, 57, 5)));
                Assert.That(workUnit.Timeout, Is.EqualTo(new DateTime(2021, 9, 7, 17, 57, 5)));
                Assert.That(workUnit.UnitStartTimeStamp, Is.EqualTo(new TimeSpan(9, 23, 36)));
                Assert.That(workUnit.Finished, Is.Null);
                Assert.That(workUnit.CoreVersion, Is.EqualTo(new Version(0, 0, 13)));
                Assert.That(workUnit.ProjectId, Is.EqualTo(18201));
                Assert.That(workUnit.ProjectRun, Is.EqualTo(44695));
                Assert.That(workUnit.ProjectClone, Is.EqualTo(3));
                Assert.That(workUnit.ProjectGen, Is.EqualTo(2));
                Assert.That(workUnit.Platform, Is.EqualTo(new WorkUnitPlatform(WorkUnitPlatformImplementation.CUDA)
                {
                    DriverVersion = "456.71",
                    ComputeVersion = "7.5",
                    CUDAVersion = "11.1"
                }));
                Assert.That(workUnit.Result, Is.EqualTo(WorkUnitResult.None));
                Assert.That(workUnit.FramesObserved, Is.EqualTo(75));
                Assert.That(workUnit.CurrentFrame!.ID, Is.EqualTo(74));
                Assert.That(workUnit.CurrentFrame.RawFramesComplete, Is.EqualTo(925000));
                Assert.That(workUnit.CurrentFrame.RawFramesTotal, Is.EqualTo(1250000));
                Assert.That(workUnit.CurrentFrame.TimeStamp, Is.EqualTo(new TimeSpan(20, 53, 54)));
                Assert.That(workUnit.CurrentFrame.Duration, Is.EqualTo(new TimeSpan(0, 2, 24)));
                Assert.That(workUnit.LogLines, Has.Count.EqualTo(181));
                Assert.That(workUnit.Core, Is.EqualTo("0x22"));
                Assert.That(workUnit.UnitHex, Is.EqualTo("0x0000000300000002000047190000ae97"));
            });
        }
    }
}

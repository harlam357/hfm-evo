using HFM.Core.WorkUnits;

namespace HFM.Core.Client;

[TestFixture]
public class FahClientResourceTests
{
    private FahClientResource? _resource;

    [TestFixture]
    public class GivenFahClientResourceHasCpuSlotDescription : FahClientResourceTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _resource = new FahClientResource
            {
                SlotDescription = new FahClientCpuSlotDescription()
            };

        [TestFixture]
        public class WhenCpuSlotDescriptionHasThreads : GivenFahClientResourceHasCpuSlotDescription
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _resource = _resource! with
                {
                    SlotDescription = new FahClientCpuSlotDescription
                    {
                        CpuThreads = 12
                    }
                };
            }

            [Test]
            public void ThenResourceTypeIncludesThreads() =>
                Assert.That(_resource!.GetResourceType(false), Is.EqualTo("CPU:12"));
        }

        [TestFixture]
        public class WhenCpuSlotDescriptionHasThreadsAndResourceHasPlatform : GivenFahClientResourceHasCpuSlotDescription
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _resource = _resource! with
                {
                    SlotDescription = new FahClientCpuSlotDescription
                    {
                        CpuThreads = 12
                    },
                    Platform = new ClientPlatform("8.0.0", "Linux")
                };
            }

            [Test]
            public void ThenResourceTypeIncludesThreads() =>
                Assert.That(_resource!.GetResourceType(false), Is.EqualTo("CPU:12"));

            [Test]
            public void ThenResourceTypeIncludesClientPlatformVersion() =>
                Assert.That(_resource!.GetResourceType(true), Is.EqualTo("CPU:12 (8.0.0)"));
        }
    }

    [TestFixture]
    public class GivenFahClientResourceHasGpuSlotDescription : FahClientResourceTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _resource = new FahClientResource
            {
                SlotDescription = new FahClientGpuSlotDescription()
            };

        [TestFixture]
        public class WhenGpuSlotDescriptionHasProcessor : GivenFahClientResourceHasGpuSlotDescription
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _resource = _resource! with
                {
                    SlotDescription = new FahClientGpuSlotDescription
                    {
                        Processor = "GeForce RTX 3080"
                    }
                };
            }

            [Test]
            public void ThenProcessorReportsGpuSlotDescriptionProcessor() =>
                Assert.That(_resource!.GetProcessor(false), Is.EqualTo("GeForce RTX 3080"));
        }

        [TestFixture]
        public class WhenGpuSlotDescriptionHasProcessorAndResourceHasWorkUnitPlatformImplementation : GivenFahClientResourceHasGpuSlotDescription
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();
                _resource = _resource! with
                {
                    SlotDescription = new FahClientGpuSlotDescription
                    {
                        Processor = "GeForce RTX 3080"
                    },
                    WorkUnit = new WorkUnit
                    {
                        Platform = new WorkUnitPlatform(WorkUnitPlatformImplementation.CUDA)
                        {
                            DriverVersion = "831.57"
                        }
                    }
                };
            }

            [Test]
            public void ThenProcessorReportsGpuSlotDescriptionProcessor() =>
                Assert.That(_resource!.GetProcessor(false), Is.EqualTo("GeForce RTX 3080"));

            [Test]
            public void ThenProcessorReportsGpuSlotDescriptionProcessorAndWorkUnitPlatform() =>
                Assert.That(_resource!.GetProcessor(true), Is.EqualTo("GeForce RTX 3080 (CUDA 831.57)"));
        }
    }

    [TestFixture]
    public class GivenFahClientResourceHasNoSlotDescription : FahClientResourceTests
    {
        [SetUp]
        public virtual void BeforeEach() =>
            _resource = new FahClientResource
            {
                SlotDescription = null
            };

        [Test]
        public void ThenResourceTypeIsEmptyString() =>
            Assert.That(_resource!.GetResourceType(false), Is.Empty);

        [Test]
        public void ThenProcessorIsEmptyString() =>
            Assert.That(_resource!.GetProcessor(false), Is.Empty);
    }

}

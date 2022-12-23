namespace HFM.Core.Client;

[TestFixture]
public class FahClientSlotDescriptionTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void ParseReturnsNullWhenInputIsInvalid(string? value)
    {
        var description = FahClientSlotDescription.Parse(value);
        Assert.That(description, Is.Null);
    }

    [Test]
    public void ParseReturnsCpuSlotDescription()
    {
        const string cpu = "cpu:16";
        var description = (FahClientCpuSlotDescription?)FahClientSlotDescription.Parse(cpu);
        Assert.Multiple(() =>
        {
            Assert.That(description, Is.Not.Null);
            Assert.That(description!.SlotType, Is.EqualTo(FahClientSlotType.Cpu));
            Assert.That(description.Value, Is.EqualTo(cpu));
            Assert.That(description.CpuThreads, Is.EqualTo(16));
        });
    }

    [Test]
    public void ParseReturnsGpuSlotDescriptionWithBusAndSlot()
    {
        const string gpu = "gpu:8:0 TU116 [GeForce GTX 1660 Ti]";
        var description = (FahClientGpuSlotDescription?)FahClientSlotDescription.Parse(gpu);
        Assert.Multiple(() =>
        {
            Assert.That(description, Is.Not.Null);
            Assert.That(description!.SlotType, Is.EqualTo(FahClientSlotType.Gpu));
            Assert.That(description.Value, Is.EqualTo(gpu));
            Assert.That(description.GpuBus, Is.EqualTo(8));
            Assert.That(description.GpuSlot, Is.EqualTo(0));
            Assert.That(description.GpuDevice, Is.Null);
            Assert.That(description.GpuPrefix, Is.EqualTo("TU116"));
            Assert.That(description.Processor, Is.EqualTo("GeForce GTX 1660 Ti"));
        });
    }

    [Test]
    public void ParseReturnsGpuSlotDescriptionWithDevice()
    {
        const string gpu = "gpu:0:TU104GL [Tesla T4]";
        var description = (FahClientGpuSlotDescription?)FahClientSlotDescription.Parse(gpu);
        Assert.Multiple(() =>
        {
            Assert.That(description, Is.Not.Null);
            Assert.That(description!.SlotType, Is.EqualTo(FahClientSlotType.Gpu));
            Assert.That(description.Value, Is.EqualTo(gpu));
            Assert.That(description.GpuBus, Is.Null);
            Assert.That(description.GpuSlot, Is.Null);
            Assert.That(description.GpuDevice, Is.EqualTo(0));
            Assert.That(description.GpuPrefix, Is.EqualTo("TU104GL"));
            Assert.That(description.Processor, Is.EqualTo("Tesla T4"));
        });
    }
}

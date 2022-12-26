namespace HFM.Core.Client;

[TestFixture]
public class FahClientGpuDeviceDescriptionTests
{
    [Test]
    public void ParseReturnsNullWhenInputIsNull() =>
        Assert.That(FahClientGpuDeviceDescription.Parse(null), Is.Null);

    [Test]
    public void ParseReturnsCudaDescription()
    {
        const string value = "Platform:0 Device:0 Bus:0 Slot:4 Compute:7.5 Driver:11.4";
        var description = FahClientGpuDeviceDescription.Parse(value);
        Assert.Multiple(() =>
        {
            Assert.That(description!.Platform, Is.EqualTo(0));
            Assert.That(description.Device, Is.EqualTo(0));
            Assert.That(description.Bus, Is.EqualTo(0));
            Assert.That(description.Slot, Is.EqualTo(4));
            Assert.That(description.Compute, Is.EqualTo("7.5"));
            Assert.That(description.Driver, Is.EqualTo("11.4"));
        });
    }

    [Test]
    public void ParseReturnsOpenClDescription()
    {
        const string value = "Platform:0 Device:1 Bus:5 Slot:0 Compute:1.2 Driver:3004.8";
        var description = FahClientGpuDeviceDescription.Parse(value);
        Assert.Multiple(() =>
        {
            Assert.That(description!.Platform, Is.EqualTo(0));
            Assert.That(description.Device, Is.EqualTo(1));
            Assert.That(description.Bus, Is.EqualTo(5));
            Assert.That(description.Slot, Is.EqualTo(0));
            Assert.That(description.Compute, Is.EqualTo("1.2"));
            Assert.That(description.Driver, Is.EqualTo("3004.8"));
        });
    }
}

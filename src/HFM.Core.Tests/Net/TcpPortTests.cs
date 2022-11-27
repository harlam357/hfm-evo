namespace HFM.Core.Net;

[TestFixture]
public class TcpPortTests
{
    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(65534, true)]
    [TestCase(65535, false)]
    public void TcpPortIsValid(int port, bool expected) =>
        Assert.That(TcpPort.Validate(port), Is.EqualTo(expected));
}

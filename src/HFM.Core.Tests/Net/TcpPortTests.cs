namespace HFM.Core.Net;

[TestFixture]
public class TcpPortTests
{
    [TestCase(0, ExpectedResult = false)]
    [TestCase(1, ExpectedResult = true)]
    [TestCase(65534, ExpectedResult = true)]
    [TestCase(65535, ExpectedResult = false)]
    public bool TcpPortIsValid(int port) => TcpPort.Validate(port);
}

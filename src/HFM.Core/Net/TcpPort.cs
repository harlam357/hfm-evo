namespace HFM.Core.Net;

public static class TcpPort
{
    public static bool Validate(int port) => port is > 0 and < UInt16.MaxValue;
}

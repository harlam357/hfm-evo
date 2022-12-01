namespace HFM.Core.Logging;

public class DebugLogger : Logger
{
    public static DebugLogger Instance { get; } = new();

    public DebugLogger() : base(LoggerLevel.Debug)
    {

    }

    public override void Log(LoggerLevel loggerLevel, string message, Exception? exception = null)
    {
        System.Diagnostics.Debug.WriteLine("[{0}] {1}", loggerLevel, message);
        if (exception is null)
        {
            return;
        }
        System.Diagnostics.Debug.WriteLine("[{0}] {1}: {2} {3}", loggerLevel, exception.GetType().FullName, exception.Message, exception.StackTrace);
    }
}

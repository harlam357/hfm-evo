namespace HFM.Core.Logging;

public class ConsoleLogger : Logger
{
    public static ConsoleLogger Instance { get; } = new();

    public ConsoleLogger() : base(LoggerLevel.Debug)
    {

    }

    public override void Log(LoggerLevel loggerLevel, string message, Exception? exception = null)
    {
        Console.WriteLine("[{0}] {1}", loggerLevel, message);
        if (exception is null)
        {
            return;
        }
        Console.WriteLine("[{0}] {1}: {2} {3}", loggerLevel, exception.GetType().FullName, exception.Message, exception.StackTrace);
    }
}

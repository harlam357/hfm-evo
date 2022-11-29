namespace HFM.Core.Logging;

public enum LoggerLevel
{
    Off = 0,
    Error = 2,
    Warn = 3,
    Info = 4,
    Debug = 5
}

public interface ILogger
{
    bool IsEnabled(LoggerLevel level);

    void Log(LoggerLevel loggerLevel, string message, Exception? exception = null);
}

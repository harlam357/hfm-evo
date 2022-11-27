namespace HFM.Core.Logging;

public enum LoggerLevel
{
    Off = 0,
    Error = 1,
    Warn = 2,
    Info = 3,
    Debug = 4
}

public interface ILogger
{
    bool IsEnabled(LoggerLevel level);

    void Log(LoggerLevel loggerLevel, string message, Exception? exception = null);
}

namespace HFM.Core.Logging;

public static class LoggerExtensions
{
    public static void Debug(this ILogger logger, string message, Exception? exception = null) =>
        logger.Log(LoggerLevel.Debug, message, exception);

    public static void Info(this ILogger logger, string message, Exception? exception = null) =>
        logger.Log(LoggerLevel.Info, message, exception);

    public static void Warn(this ILogger logger, string message, Exception? exception = null) =>
        logger.Log(LoggerLevel.Warn, message, exception);

    public static void Error(this ILogger logger, string message, Exception? exception = null) =>
        logger.Log(LoggerLevel.Error, message, exception);
}

using System.Globalization;

namespace HFM.Core.Logging;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class LoggerExtensions
{
    public static void Debug(this ILogger logger, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Debug, String.Format(CultureInfo.CurrentCulture, message, args));

    public static void Debug(this ILogger logger, Exception? exception, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Debug, String.Format(CultureInfo.CurrentCulture, message, args), exception);

    public static void Info(this ILogger logger, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Info, String.Format(CultureInfo.CurrentCulture, message, args));

    public static void Info(this ILogger logger, Exception? exception, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Info, String.Format(CultureInfo.CurrentCulture, message, args), exception);

    public static void Warn(this ILogger logger, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Warn, String.Format(CultureInfo.CurrentCulture, message, args));

    public static void Warn(this ILogger logger, Exception? exception, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Warn, String.Format(CultureInfo.CurrentCulture, message, args), exception);

    public static void Error(this ILogger logger, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Error, String.Format(CultureInfo.CurrentCulture, message, args));

    public static void Error(this ILogger logger, Exception? exception, string message, params object?[] args) =>
        logger.Log(LoggerLevel.Error, String.Format(CultureInfo.CurrentCulture, message, args), exception);
}

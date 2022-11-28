namespace HFM.Core.Logging;

public abstract class Logger : ILogger
{
    public const string NameFormat = "({0}) {1}";

    protected Logger(LoggerLevel loggerLevel)
    {
        Level = loggerLevel;
    }

    public LoggerLevel Level { get; set; }

    public bool IsEnabled(LoggerLevel level) => level <= Level;

    public abstract void Log(LoggerLevel loggerLevel, string message, Exception? exception = null);

    protected static string FormatMessage(LoggerLevel loggerLevel, string message, DateTime now) =>
        $"[{now.ToShortDateString()}-{now.ToLongTimeString()}] {ToLevelIdentifier(loggerLevel)} {message}";

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected static string ToLevelIdentifier(LoggerLevel loggerLevel) =>
        loggerLevel switch
        {
            LoggerLevel.Off => " ",
            LoggerLevel.Error => "X",
            LoggerLevel.Warn => "!",
            LoggerLevel.Info => "-",
            LoggerLevel.Debug => "+",
            _ => String.Empty
        };
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullLogger : Logger
{
    public static NullLogger Instance { get; } = new();

    private NullLogger() : base(default)
    {

    }

    public override void Log(LoggerLevel loggerLevel, string message, Exception? exception = null)
    {

    }
}

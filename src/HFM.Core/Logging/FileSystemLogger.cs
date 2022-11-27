using System.Diagnostics;
using System.Globalization;

namespace HFM.Core.Logging;

public class FileSystemLogger : Logger, ILoggerEvents
{
    public string Path { get; }

    public FileSystemLogger(string path) : base(LoggerLevel.Info)
    {
        Path = path;
    }

    private static readonly object _LogLock = new();

    public override void Log(LoggerLevel loggerLevel, string message, Exception? exception = null)
    {
        if (!IsEnabled(loggerLevel)) return;

        lock (_LogLock)
        {
            OnLogged(loggerLevel, message);
            OnExceptionLogged(loggerLevel, exception);
        }
    }

    public event EventHandler<LoggedEventArgs>? Logged;

    protected virtual void OnLogged(LoggerLevel loggerLevel, string message)
    {
        var now = DateTime.Now;

        var messages = message
            .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            .Select(x => FormatMessage(loggerLevel, x, now))
            .ToList();

        Logged?.Invoke(this, new LoggedEventArgs(messages));

        foreach (var m in messages)
        {
            Trace.WriteLine(m);
        }
    }

    public event EventHandler<LoggedEventArgs>? ExceptionLogged;

    protected virtual void OnExceptionLogged(LoggerLevel loggerLevel, Exception? exception)
    {
        if (exception is null) return;

        var now = DateTime.Now;
        string m = FormatMessage(loggerLevel, exception.ToString(), now);

        ExceptionLogged?.Invoke(this, new LoggedEventArgs(new[] { m }));
        Trace.WriteLine(m);
    }

    public void Initialize()
    {
        try
        {
            InitializeInternal();
        }
        catch (IOException ex)
        {
            string message = String.Format(CultureInfo.CurrentCulture,
                "Logging failed to initialize. Please check to be sure that the {0} or {1} file is not open or otherwise in use.",
                LogFileName, PreviousLogFileName);
            throw new InvalidOperationException(message, ex);
        }
    }

    private void InitializeInternal()
    {
        EnsureDirectoryExists();
        MoveLogFileToPreviousLogFileIfMaxSizeIsExceeded();
        AddTraceListener();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(Path))
        {
            Directory.CreateDirectory(Path);
        }
    }

    public const int MaxLogFileSize = 512000;
    public const string LogFileName = "HFM.log";
    public const string PreviousLogFileName = "HFM-prev.log";

    private void MoveLogFileToPreviousLogFileIfMaxSizeIsExceeded()
    {
        string logFilePath = GetLogFilePath(LogFileName);
        string prevLogFilePath = GetLogFilePath(PreviousLogFileName);

        var fi = new FileInfo(logFilePath);
        if (fi.Exists && fi.Length > MaxLogFileSize)
        {
            var fi2 = new FileInfo(prevLogFilePath);
            if (fi2.Exists)
            {
                fi2.Delete();
            }
            fi.MoveTo(prevLogFilePath);
        }
    }

    private void AddTraceListener()
    {
        string logFilePath = GetLogFilePath(LogFileName);
        Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
        Trace.AutoFlush = true;
    }

    private string GetLogFilePath(string fileName) => System.IO.Path.Combine(Path, fileName);
}

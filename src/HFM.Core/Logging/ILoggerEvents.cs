namespace HFM.Core.Logging;

public class LoggedEventArgs : EventArgs
{
    public ICollection<string> Messages { get; }

    public LoggedEventArgs(ICollection<string> messages)
    {
        Messages = messages;
    }
}

public interface ILoggerEvents
{
    event EventHandler<LoggedEventArgs> Logged;
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class NullLoggerEvents : ILoggerEvents
{
    public static NullLoggerEvents Instance { get; } = new();

#pragma warning disable 0067
    public event EventHandler<LoggedEventArgs>? Logged;
#pragma warning restore 0067
}

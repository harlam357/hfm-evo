namespace HFM.Core.Logging;

[TestFixture]
public class LoggerExtensionsTests
{
    private ILogger? _logger;
    private string? _message;

    [SetUp]
    public void BeforeEach()
    {
        var logger = new FileSystemLogger("")
        {
            Level = LoggerLevel.Debug
        };
        logger.Logged += (_, e) => _message = e.Messages.First();
        _logger = logger;
    }

    [Test]
    public void LogError()
    {
        _logger!.Error("Error");
        Assert.That(_message, Contains.Substring(" X "));
    }

    [Test]
    public void LogWarning()
    {
        _logger!.Warn("Warning");
        Assert.That(_message, Contains.Substring(" ! "));
    }

    [Test]
    public void LogInformation()
    {
        _logger!.Info("Information");
        Assert.That(_message, Contains.Substring(" - "));
    }

    [Test]
    public void LogDebug()
    {
        _logger!.Debug("Debug");
        Assert.That(_message, Contains.Substring(" + "));
    }
}

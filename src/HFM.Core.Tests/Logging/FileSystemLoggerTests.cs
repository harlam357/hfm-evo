namespace HFM.Core.Logging;

[TestFixture]
public class FileSystemLoggerTests
{
    private FileSystemLogger? _logger;

    [TestFixture]
    public class GivenFileSystemLogger : FileSystemLoggerTests
    {
        [SetUp]
        public void BeforeEach() => _logger = new FileSystemLogger("");

        [Test]
        public void DefaultLevelIsInfo() => Assert.That(_logger!.Level, Is.EqualTo(LoggerLevel.Info));

        [TestCase(LoggerLevel.Off, ExpectedResult = true)]
        [TestCase(LoggerLevel.Error, ExpectedResult = true)]
        [TestCase(LoggerLevel.Warn, ExpectedResult = true)]
        [TestCase(LoggerLevel.Info, ExpectedResult = true)]
        [TestCase(LoggerLevel.Debug, ExpectedResult = false)]
        public bool IsEnabled(LoggerLevel level) => _logger!.IsEnabled(level);

        [Test]
        public void RaisesLoggedEventWhenLevelIsEnabled()
        {
            bool raised = false;
            _logger!.Logged += (_, _) => raised = true;
            _logger.Log(LoggerLevel.Info, "Test");
            Assert.That(raised, Is.True);
        }

        [Test]
        public void DoesNotRaiseLoggedEventWhenLevelIsNotEnabled()
        {
            bool raised = false;
            _logger!.Logged += (_, _) => raised = true;
            _logger.Log(LoggerLevel.Debug, "Test");
            Assert.That(raised, Is.False);
        }

        [Test]
        public void RaisesExceptionLoggedEventWhenLevelIsEnabled()
        {
            bool raised = false;
            _logger!.ExceptionLogged += (_, _) => raised = true;
            _logger.Log(LoggerLevel.Info, "Test", new InvalidOperationException("Test"));
            Assert.That(raised, Is.True);
        }

        [Test]
        public void DoesNotRaiseExceptionLoggedEventWhenLevelIsNotEnabled()
        {
            bool raised = false;
            _logger!.ExceptionLogged += (_, _) => raised = true;
            _logger.Log(LoggerLevel.Debug, "Test", new InvalidOperationException("Test"));
            Assert.That(raised, Is.False);
        }

        [Test]
        public void SplitsLoggedMessageUsingSystemSpecificNewLine()
        {
            LoggedEventArgs? args = null;
            _logger!.Logged += (_, e) => args = e;

            string message = $"First{Environment.NewLine}Second{Environment.NewLine}Third";
            _logger.Log(LoggerLevel.Info, message);
            Assert.That(args!.Messages, Has.Count.EqualTo(3));
        }
    }

    [TestFixture]
    public class GivenPathExists : FileSystemLoggerTests, IDisposable
    {
        private ArtifactFolder? _artifacts;

        [SetUp]
        public virtual void BeforeEach()
        {
            _artifacts = new ArtifactFolder();
            _logger = new FileSystemLogger(_artifacts.Path);
        }

        [TearDown]
        public void AfterEach() => _artifacts?.Dispose();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _artifacts?.Dispose();
        }

        [TestFixture]
        public class AndNoExistingLogFilesArePresent : GivenPathExists
        {
            [Test]
            public void InitializesLoggingToFileSystem()
            {
                _logger!.Initialize();
                _logger.Log(LoggerLevel.Info, "Test");

                string path = Path.Combine(_artifacts!.Path, FileSystemLogger.LogFileName);
                Assert.That(File.Exists(path));
            }
        }

        [TestFixture]
        public class AndLogFileIsGreaterThanMaximumSize : GivenPathExists
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                string logFilePath = Path.Combine(_artifacts!.Path, FileSystemLogger.LogFileName);
                FillLogFile(logFilePath, FileSystemLogger.MaxLogFileSize + 1);
            }

            [Test]
            public void MovesLogFileToPreviousLogFileIfMaxSizeIsExceeded()
            {
                _logger!.Initialize();

                string prevLogFilePath = Path.Combine(_artifacts!.Path, FileSystemLogger.PreviousLogFileName);
                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(prevLogFilePath));
                    Assert.That(new FileInfo(prevLogFilePath).Length, Is.EqualTo(FileSystemLogger.MaxLogFileSize + 1));
                });
            }
        }

        [TestFixture]
        public class AndLogFileIsGreaterThanMaximumSizeAndPreviousLogExists : GivenPathExists
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                string logFilePath = Path.Combine(_artifacts!.Path, FileSystemLogger.LogFileName);
                FillLogFile(logFilePath, FileSystemLogger.MaxLogFileSize + 1);

                string prevLogFilePath = Path.Combine(_artifacts!.Path, FileSystemLogger.PreviousLogFileName);
                FillLogFile(prevLogFilePath, 1);
            }

            [Test]
            public void MovesLogFileToPreviousLogFileIfMaxSizeIsExceeded()
            {
                _logger!.Initialize();

                string prevLogFilePath = Path.Combine(_artifacts!.Path, FileSystemLogger.PreviousLogFileName);
                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(prevLogFilePath));
                    Assert.That(new FileInfo(prevLogFilePath).Length, Is.EqualTo(FileSystemLogger.MaxLogFileSize + 1));
                });
            }
        }

        [TestFixture]
        public class AndExistingLogFileIsLocked : GivenPathExists
        {
            [SetUp]
            public override void BeforeEach()
            {
                base.BeforeEach();

                string logFilePath = Path.Combine(_artifacts!.Path, FileSystemLogger.LogFileName);
                FillLogFile(logFilePath, FileSystemLogger.MaxLogFileSize + 1);

                _ = File.Open(logFilePath, FileMode.Open);
            }

            [Test]
            public void ThenInitializeThrows() =>
                Assert.That(() => _logger!.Initialize(), Throws.InvalidOperationException);
        }

        private static void FillLogFile(string path, int length)
        {
            using var stream = File.Create(path);
            stream.Write(Enumerable.Repeat((byte)0, length).ToArray());
            stream.Flush();
        }
    }

    [TestFixture]
    public class GivenPathDoesNotExist : FileSystemLoggerTests
    {
        private string? _path;

        [SetUp]
        public void BeforeEach()
        {
            _path = ArtifactFolder.GetRandomPath(Environment.CurrentDirectory);
            _logger = new FileSystemLogger(_path);
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                Directory.Delete(_path!, true);
            }
            catch
            {
                // do nothing
            }
        }

        [Test]
        public void InitializesLoggingToFileSystem()
        {
            _logger!.Initialize();
            _logger.Log(LoggerLevel.Info, "Test");

            string path = Path.Combine(_path!, "HFM.log");
            Assert.That(File.Exists(path));
        }
    }
}

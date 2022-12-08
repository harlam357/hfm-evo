using HFM.Core.Logging;

namespace HFM.Core.ScheduledTasks;

[TestFixture]
public class DelegateScheduledTaskTests
{
    private static ILogger Logger => TestLogger.Instance;

    [Test]
    public void TaskIntervalIsSet()
    {
        using var task = new DelegateScheduledTask("Task", 5000, ct => ct.ThrowIfCancellationRequested());
        Assert.That(task.Interval, Is.EqualTo(5000));
    }

    [Test]
    public async Task RunsTask()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, _ => Thread.Sleep(10));
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Run();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();

        // stop the task
        task.Stop();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task RunsTaskWhileAlreadyRunning()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, _ => Thread.Sleep(10));
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Run();
        Assert.That(task.Enabled, Is.True);
        task.Run();

        // stop the task
        task.Stop();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task StartsAndRunsTask()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, _ => Thread.Sleep(10));
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();

        // stop the task
        task.Stop();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task StartsTaskWhileAlreadyRunning()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, _ => Thread.Sleep(10));
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();
        task.Start();

        // stop the task
        task.Stop();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task RestartsAndRunsTask()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, _ => Thread.Sleep(10));
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        task.Restart();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();

        // stop the task
        task.Stop();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task RestartsTaskWhileAlreadyRunning()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, _ => Thread.Sleep(10));
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();
        task.Restart();

        // stop the task
        task.Stop();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task StartsTaskWithCancellation()
    {
        // create and start the task
        using var task = new DelegateScheduledTask("Task", 100, ct =>
        {
            while (!ct.IsCancellationRequested)
            {
                Thread.Sleep(10);
                ct.ThrowIfCancellationRequested();
            }
        });
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();

        // cancel the task
        task.Cancel();
        Assert.That(task.Enabled, Is.False);

        try
        {
            // wait for completion
            await task.InnerTask!;
        }
        catch (OperationCanceledException ex)
        {
            Logger.Debug(ex.ToString());
        }
    }

    [Test]
    public async Task StartsAndRunsTaskAndCancelsAfterCompletion()
    {
        // create and start the task
        var task = new DelegateScheduledTask("Task", 10, ct =>
        {
            Thread.Sleep(1000);
        });
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();

        // cancel the task
        task.Cancel();
        Assert.That(task.Enabled, Is.False);

        // wait for completion
        await task.InnerTask!;
    }

    [Test]
    public async Task StartsAndRunsTaskThatThrowsException()
    {
        // create and start the task
        using var task = new DelegateScheduledTaskWithRunCount("Task", 100, _ =>
        {
            Thread.Sleep(10);
            throw new InvalidOperationException("test exception");
        });
        task.Changed += TaskChanged;
        Assert.That(task.Enabled, Is.False);

        task.Start();
        Assert.That(task.Enabled, Is.True);

        await AllowTimeForTheTaskToBeScheduledAndStarted();

        // stop the task
        task.Stop();
        Assert.Multiple(() =>
        {
            Assert.That(task.Enabled, Is.False);
            Assert.That(task.Exception, Is.Not.Null);
        });

        try
        {
            // wait for completion
            await task.InnerTask!;
        }
        catch (Exception ex)
        {
            Logger.Debug(ex.ToString());
        }

        Assert.That(task.RunCount, Is.GreaterThan(1), "RunCount should be greater than 1");
    }

    private static Task AllowTimeForTheTaskToBeScheduledAndStarted() => Task.Delay(1000);

    private static void TaskChanged(object? sender, ScheduledTaskChangedEventArgs e) => Logger.Debug(e.ToString()!);

    private sealed class DelegateScheduledTaskWithRunCount : DelegateScheduledTask
    {
        public DelegateScheduledTaskWithRunCount(string name, double interval, Action<CancellationToken> action) : base(name, interval, action)
        {
        }

        public int RunCount { get; private set; }

        protected override void OnRun(CancellationToken ct)
        {
            RunCount++;
            base.OnRun(ct);
        }
    }
}

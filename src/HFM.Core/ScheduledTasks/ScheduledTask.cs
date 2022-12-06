﻿using System.Diagnostics;

namespace HFM.Core.ScheduledTasks;

public interface IScheduledTaskInfo
{
    string Name { get; }

    bool Enabled { get; }

    bool InProgress { get; }
}

public abstract class ScheduledTask : IScheduledTaskInfo, IDisposable
{
    private readonly System.Timers.Timer _timer;

    protected ScheduledTask(string name)
    {
        Name = name;
        _timer = new System.Timers.Timer();
        _timer.Elapsed += (_, _) => Run();
    }

    public event EventHandler<ScheduledTaskChangedEventArgs>? Changed;

    protected virtual void OnTaskChanged(ScheduledTaskChangedAction action, double? interval = null) =>
        Changed?.Invoke(this, new ScheduledTaskChangedEventArgs(this, action, interval));

    public string Name { get; }

    public double Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public bool Enabled { get; private set; }

    public bool InProgress => InnerTask is { Status: < TaskStatus.RanToCompletion };

    public Exception? Exception { get; private set; }

    internal Task? InnerTask { get; private set; }

    private CancellationTokenSource? _cts;

    public void Run() => Run(true);

    public void Run(bool enabled)
    {
        Enabled |= enabled;
        if (InProgress)
        {
            OnTaskChanged(ScheduledTaskChangedAction.AlreadyInProgress);
            return;
        }

        var sw = Stopwatch.StartNew();

        _timer.Stop();
        OnTaskChanged(ScheduledTaskChangedAction.Running);

        var lastCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        lastCts?.Dispose();

        InnerTask = Task.Run(() => OnRun(_cts.Token), _cts.Token);
        InnerTask.ContinueWith(t =>
        {
            switch (t.Status)
            {
                case TaskStatus.Faulted:
                    Exception = t.Exception?.InnerException;
                    OnTaskChanged(ScheduledTaskChangedAction.Faulted, sw.ElapsedMilliseconds);
                    CancelOrStart();
                    break;
                case TaskStatus.Canceled:
                    OnTaskChanged(ScheduledTaskChangedAction.Canceled);
                    break;
                case TaskStatus.RanToCompletion:
                    OnTaskChanged(ScheduledTaskChangedAction.Finished, sw.ElapsedMilliseconds);
                    CancelOrStart();
                    break;
            }
        }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

        void CancelOrStart()
        {
            if (_cts.Token.IsCancellationRequested)
            {
                OnTaskChanged(ScheduledTaskChangedAction.Canceled);
            }
            else if (Enabled)
            {
                Start();
            }
        }
    }

    protected abstract void OnRun(CancellationToken ct);

    public void Start()
    {
        Enabled = true;
        if (InProgress)
        {
            OnTaskChanged(ScheduledTaskChangedAction.AlreadyInProgress);
            return;
        }

        if (!_timer.Enabled)
        {
            _timer.Start();
            OnTaskChanged(ScheduledTaskChangedAction.Started, _timer.Interval);
        }
    }

    public void Restart()
    {
        Enabled = true;
        if (InProgress)
        {
            OnTaskChanged(ScheduledTaskChangedAction.AlreadyInProgress);
            return;
        }

        if (_timer.Enabled)
        {
            _timer.Stop();
        }
        _timer.Start();
        OnTaskChanged(ScheduledTaskChangedAction.Started, _timer.Interval);
    }

    public void Stop()
    {
        Enabled = false;
        if (_timer.Enabled)
        {
            _timer.Stop();
            OnTaskChanged(ScheduledTaskChangedAction.Stopped);
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
        Stop();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _timer.Dispose();
                _cts?.Dispose();
                InnerTask?.Dispose();
            }
        }

        _disposed = true;
    }
}

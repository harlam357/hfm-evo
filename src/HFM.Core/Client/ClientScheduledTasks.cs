using HFM.Core.Artifacts;
using HFM.Core.Logging;
using HFM.Core.ScheduledTasks;
using HFM.Preferences;

namespace HFM.Core.Client;

public class ClientScheduledTasks : IDisposable
{
    public const string ClientTaskName = "Client Refresh";
    public const string WebTaskName = "Web Generation";

    private readonly ILogger _logger;
    private readonly IPreferences _preferences;
    private readonly ClientConfiguration _clientConfiguration;
    private readonly IWebArtifactDeployment _webArtifactDeployment;

    private readonly ScheduledTask _clientRefreshTask;
    private readonly ScheduledTask _webArtifactsTask;

    public ClientScheduledTasks(ILogger logger, IPreferences preferences, ClientConfiguration clientConfiguration, IWebArtifactDeployment webArtifactDeployment)
    {
        _logger = logger;
        _preferences = preferences;
        _clientConfiguration = clientConfiguration;
        _webArtifactDeployment = webArtifactDeployment;

        _preferences.PreferenceChanged += OnPreferenceChanged;
        _clientConfiguration.ClientConfigurationChanged += OnClientConfigurationChanged;

        _clientRefreshTask = new DelegateScheduledTask(ClientTaskName, ClientRefreshTaskInterval, ClientRefreshAction);
        _clientRefreshTask.Changed += TaskChanged;
        _webArtifactsTask = new DelegateScheduledTask(WebTaskName, WebArtifactsTaskInterval, WebGenerationAction);
        _webArtifactsTask.Changed += TaskChanged;
    }

    public IScheduledTaskInfo ClientRefreshTask => _clientRefreshTask;

    public IScheduledTaskInfo WebArtifactsTask => _webArtifactsTask;

    private void OnPreferenceChanged(object? sender, PreferenceChangedEventArgs e)
    {
#pragma warning disable IDE0010 // Add missing cases
        switch (e.Preference)
        {
            case Preference.ClientRetrievalTask:
                if (!RestartClientRefreshTaskIfEnabled())
                {
                    StopClientRefreshTask();
                }
                break;
            case Preference.WebGenerationTask:
                if (!RestartWebArtifactsTaskIfEnabledOnInterval())
                {
                    StopWebArtifactsTask();
                }
                break;
            case Preference.PPDCalculation:
                RunClientRefreshTask();
                break;
        }
#pragma warning restore IDE0010 // Add missing cases
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private void OnClientConfigurationChanged(object? sender, ClientConfigurationChangedEventArgs e)
    {
        switch (e.Action)
        {
            case ClientConfigurationChangedAction.Add:
                RunClientRefreshTask(ClientRefreshTaskEnabled);
                StartWebArtifactsTaskIfEnabledOnInterval();
                break;
            case ClientConfigurationChangedAction.Remove:
            case ClientConfigurationChangedAction.Clear:
                KillAllTasksIfConfigurationIsEmpty();
                break;
            case ClientConfigurationChangedAction.Edit:
            case ClientConfigurationChangedAction.Refresh:
                RunClientRefreshTask();
                break;
            case ClientConfigurationChangedAction.Invalidate:
            default:
                // do nothing
                break;
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private void TaskChanged(object? sender, ScheduledTaskChangedEventArgs e)
    {
        switch (e.Action)
        {
            case ScheduledTaskChangedAction.Started:
                _logger.Info(e.ToString(i => $"{(int)TimeSpan.FromMilliseconds(i.GetValueOrDefault()).TotalMinutes} minutes")!);
                break;
            case ScheduledTaskChangedAction.Faulted:
                _logger.Error(e.ToString()!);
                break;
            case ScheduledTaskChangedAction.AlreadyInProgress:
                _logger.Warn(e.ToString()!);
                break;
            case ScheduledTaskChangedAction.Stopped:
            case ScheduledTaskChangedAction.Running:
            case ScheduledTaskChangedAction.Canceled:
            case ScheduledTaskChangedAction.Finished:
            default:
                _logger.Info(e.ToString()!);
                break;
        }
    }

    private double ClientRefreshTaskInterval =>
        TimeSpan.FromMinutes(_preferences.Get<int>(Preference.ClientRetrievalTaskInterval)).TotalMilliseconds;

    private double WebArtifactsTaskInterval =>
        TimeSpan.FromMinutes(_preferences.Get<int>(Preference.WebGenerationTaskInterval)).TotalMilliseconds;

    private bool ClientRefreshTaskEnabled =>
        _preferences.Get<bool>(Preference.ClientRetrievalTaskEnabled);

    private bool RunWebArtifactsTaskAfterClientRefreshTask =>
        _preferences.Get<bool>(Preference.WebGenerationTaskEnabled) &&
        _preferences.Get<bool>(Preference.WebGenerationTaskAfterClientRetrieval);

    private bool RunWebArtifactsTaskOnInterval =>
        _preferences.Get<bool>(Preference.WebGenerationTaskEnabled) &&
        _preferences.Get<bool>(Preference.WebGenerationTaskAfterClientRetrieval) == false;

    private void KillAllTasksIfConfigurationIsEmpty()
    {
        if ((_clientRefreshTask.Enabled || _webArtifactsTask.Enabled) && _clientConfiguration.Count == 0)
        {
            _logger.Info("No clients... stopping all scheduled tasks");
            _clientRefreshTask.Cancel();
            _webArtifactsTask.Cancel();
        }
    }

    private void RunClientRefreshTask(bool enabled = false)
    {
        _clientRefreshTask.Interval = ClientRefreshTaskInterval;
        _clientRefreshTask.Run(enabled);
    }

    private void StartWebArtifactsTaskIfEnabledOnInterval()
    {
        if (RunWebArtifactsTaskOnInterval)
        {
            _webArtifactsTask.Interval = WebArtifactsTaskInterval;
            _webArtifactsTask.Start();
        }
    }

    private bool RestartClientRefreshTaskIfEnabled()
    {
        var enabled = ClientRefreshTaskEnabled;
        if (enabled && _clientConfiguration.Count != 0)
        {
            _clientRefreshTask.Interval = ClientRefreshTaskInterval;
            _clientRefreshTask.Restart();
        }

        return enabled;
    }

    private void StopClientRefreshTask() => _clientRefreshTask.Stop();

    private bool RestartWebArtifactsTaskIfEnabledOnInterval()
    {
        var enabled = RunWebArtifactsTaskOnInterval;
        if (enabled && _clientConfiguration.Count != 0)
        {
            _webArtifactsTask.Interval = WebArtifactsTaskInterval;
            _webArtifactsTask.Restart();
        }

        return enabled;
    }

    private void StopWebArtifactsTask() => _webArtifactsTask.Stop();

    private void ClientRefreshAction(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var clients = _clientConfiguration.ToList();
        Parallel.ForEach(clients, x =>
        {
            ct.ThrowIfCancellationRequested();
            x.Refresh(ct);
        });

        if (RunWebArtifactsTaskAfterClientRefreshTask)
        {
            ct.ThrowIfCancellationRequested();
            _webArtifactsTask.Run(false);
        }
    }

    private void WebGenerationAction(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _webArtifactDeployment.Deploy(ct);
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
                _clientRefreshTask.Dispose();
                _webArtifactsTask.Dispose();
            }
        }

        _disposed = true;
    }
}

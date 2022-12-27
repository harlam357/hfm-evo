namespace HFM.Core.Client;

/// <summary>
/// Represents the status of a Folding@Home client resource.
/// </summary>
public enum ClientResourceStatus
{
    /// <summary>
    /// The status of the resource is unknown.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The resource is paused.
    /// </summary>
    Paused = 1,
    /// <summary>
    /// The resource is running.
    /// </summary>
    Running = 2,
    /// <summary>
    /// The resource is finishing.
    /// </summary>
    Finishing = 3,
    /// <summary>
    /// The resource is ready for work.
    /// </summary>
    Ready = 4,
    /// <summary>
    /// The resource is stopping.
    /// </summary>
    Stopping = 5,
    /// <summary>
    /// The resource work has failed.
    /// </summary>
    Failed = 6,
    /// <summary>
    /// The resource is running but does not have enough frame data to calculate a frame time.
    /// </summary>
    RunningNoFrameTimes = 7,
    /// <summary>
    /// The resource is offline.
    /// </summary>
    Offline = 8,
    /// <summary>
    /// The resource is disabled.
    /// </summary>
    Disabled = 9
}

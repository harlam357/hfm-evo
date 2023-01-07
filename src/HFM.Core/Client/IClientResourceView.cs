namespace HFM.Core.Client;

public interface IClientResourceView
{
    ClientResourceStatus Status { get; }
    int Progress { get; }
    string? Name { get; }
    string ResourceType { get; }
    string Processor { get; }
    TimeSpan FrameTime { get; }
    double PointsPerDay { get; }
    string ETA { get; }
    string Core { get; }
    string ProjectRunCloneGen { get; }
    double Credit { get; }
    string DonorIdentity { get; }
    DateTime Assigned { get; }
    DateTime Timeout { get; }
}

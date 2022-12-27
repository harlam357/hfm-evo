using System.Drawing;

namespace HFM.Core.Client;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ClientResourceStatusExtensions
{
    public static Color GetStatusColor(this ClientResourceStatus status) =>
        status switch
        {
            ClientResourceStatus.Running => Color.Green,
            ClientResourceStatus.RunningNoFrameTimes => Color.Yellow,
            ClientResourceStatus.Finishing => Color.DarkSeaGreen,
            ClientResourceStatus.Ready => Color.DarkCyan,
            ClientResourceStatus.Stopping => Color.DarkRed,
            ClientResourceStatus.Failed => Color.DarkRed,
            ClientResourceStatus.Paused => Color.Orange,
            ClientResourceStatus.Offline => Color.Gray,
            ClientResourceStatus.Disabled => Color.DimGray,
            ClientResourceStatus.Unknown => Color.Gray,
            _ => Color.Gray
        };

#pragma warning disable IDE0072 // Add missing cases
    public static bool IsOnline(this ClientResourceStatus status) =>
        status switch
        {
            ClientResourceStatus.Paused => true,
            ClientResourceStatus.Running => true,
            ClientResourceStatus.Finishing => true,
            ClientResourceStatus.Ready => true,
            ClientResourceStatus.Stopping => true,
            ClientResourceStatus.Failed => true,
            ClientResourceStatus.RunningNoFrameTimes => true,
            _ => false
        };

    public static bool IsRunning(this ClientResourceStatus status) =>
        status switch
        {
            ClientResourceStatus.Running => true,
            ClientResourceStatus.Finishing => true,
            ClientResourceStatus.RunningNoFrameTimes => true,
            _ => false
        };

    public static string ToUserString(this ClientResourceStatus status) =>
        status switch
        {
            ClientResourceStatus.RunningNoFrameTimes => "Running with Benchmark TPF",
            _ => status.ToString()
        };
#pragma warning restore IDE0072 // Add missing cases
}

namespace HFM.Core;

/// <summary>
/// Provides progress information.
/// </summary>
public readonly record struct ProgressInfo(int Percent, string? Message);

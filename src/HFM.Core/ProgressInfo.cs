namespace HFM.Core;

/// <summary>
/// Provides progress information.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public readonly record struct ProgressInfo(int Percent, string? Message);

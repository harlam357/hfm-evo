namespace HFM.Core.WorkUnits;

public static class WorkUnitPlatformImplementation
{
    public const string CPU = nameof(CPU);
    public const string CUDA = nameof(CUDA);
    public const string OpenCL = nameof(OpenCL);
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public record WorkUnitPlatform(string Implementation)
{
    public string? DriverVersion { get; init; }

    public string? ComputeVersion { get; init; }

    public string? CUDAVersion { get; init; }
}

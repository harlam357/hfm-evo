namespace HFM.Core.Artifacts;

public interface IArtifactDeployment
{
    void Deploy(CancellationToken cancellationToken = default);
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullArtifactDeployment : IArtifactDeployment
{
    public static NullArtifactDeployment Instance { get; } = new();

    public void Deploy(CancellationToken cancellationToken = default)
    {
        // do nothing
    }
}

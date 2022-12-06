namespace HFM.Core.Artifacts;

public interface IWebArtifactDeployment
{
    void Deploy(CancellationToken cancellationToken = default);
}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullWebArtifactDeployment : IWebArtifactDeployment
{
    public static NullWebArtifactDeployment Instance { get; } = new();

    public void Deploy(CancellationToken cancellationToken = default)
    {
        // do nothing
    }
}

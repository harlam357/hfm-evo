namespace HFM.Core.Artifacts;

public interface IWebGenerationArtifactDeployment : IArtifactDeployment
{

}

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class NullWebGenerationArtifactDeployment : IWebGenerationArtifactDeployment
{
    public static NullWebGenerationArtifactDeployment Instance { get; } = new();

    public void Deploy(CancellationToken cancellationToken = default)
    {
        // do nothing
    }
}

using HFM.Core.Artifacts;

using LightInject;

namespace HFM.Console.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class ArtifactsCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton((IWebGenerationArtifactDeployment)NullWebGenerationArtifactDeployment.Instance);
    }
}

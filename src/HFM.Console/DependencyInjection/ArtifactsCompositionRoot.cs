using HFM.Core.Artifacts;

using LightInject;

namespace HFM.Console.DependencyInjection;

internal sealed class ArtifactsCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton<IWebArtifactDeployment, NullWebArtifactDeployment>();
    }
}

using HFM.Core.Artifacts;

using LightInject;

namespace HFM.Console.DependencyInjection;

internal class ArtifactsCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton<IWebArtifactDeployment, NullWebArtifactDeployment>();
    }
}

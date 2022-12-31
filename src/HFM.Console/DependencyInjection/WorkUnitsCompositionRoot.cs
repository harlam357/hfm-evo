using HFM.Core.WorkUnits;

using LightInject;

namespace HFM.Console.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class WorkUnitsCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.AddSingleton<ProteinDataContainer>();
        serviceRegistry.Initialize<ProteinDataContainer>((_, instance) => instance.Read());
        serviceRegistry.AddSingleton<IProjectSummaryService, ProjectSummaryService>();
        serviceRegistry.AddSingleton<IProteinService, ProteinService>();
    }
}

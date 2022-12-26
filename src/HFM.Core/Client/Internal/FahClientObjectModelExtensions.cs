using HFM.Client.ObjectModel;
using HFM.Core.WorkUnits;

namespace HFM.Core.Client.Internal;

internal static class FahClientObjectModelExtensions
{
    internal static ProjectInfo ToProjectInfo(this Unit unit) =>
        new()
        {
            ProjectId = unit.Project.GetValueOrDefault(),
            ProjectRun = unit.Run.GetValueOrDefault(),
            ProjectClone = unit.Clone.GetValueOrDefault(),
            ProjectGen = unit.Gen.GetValueOrDefault()
        };
}

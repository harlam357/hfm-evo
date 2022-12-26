using HFM.Core.WorkUnits;
using HFM.Log;

namespace HFM.Core.Internal;

internal static class LogExtensions
{
    internal static SlotRun? GetSlotRun(this ClientRun? clientRun, int slotId) =>
        clientRun is not null && clientRun.SlotRuns.TryGetValue(slotId, out var slotRun)
            ? slotRun
            : null;

    internal static UnitRun? GetUnitRun(this ClientRun? clientRun, int slotId, int queueIndex, IProjectInfo projectInfo) =>
        clientRun.GetSlotRun(slotId)?.UnitRuns.LastOrDefault(x => x.QueueIndex == queueIndex && projectInfo.EqualsProject(x.Data.ToProjectInfo()));

    internal static ProjectInfo ToProjectInfo(this UnitRunData data) =>
        new()
        {
            ProjectId = data.ProjectID,
            ProjectRun = data.ProjectRun,
            ProjectClone = data.ProjectClone,
            ProjectGen = data.ProjectGen
        };
}

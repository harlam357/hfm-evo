using System.Globalization;

using HFM.Client.ObjectModel;
using HFM.Core.Client.Internal;
using HFM.Core.Internal;
using HFM.Core.WorkUnits;
using HFM.Log;
using HFM.Proteins;

namespace HFM.Core.Client;

public class FahClientWorkUnitCollectionBuilder
{
    private readonly FahClientMessages _messages;
    private readonly IProteinService _proteinService;

    public FahClientWorkUnitCollectionBuilder(FahClientMessages messages, IProteinService proteinService)
    {
        _messages = messages;
        _proteinService = proteinService;
    }

    public async Task<WorkUnitCollection> BuildForSlot(int slotId, FahClientSlotDescription slotDescription, WorkUnit? previousWorkUnit)
    {
        var workUnits = new WorkUnitCollection();
        if (_messages.UnitCollection is null)
        {
            return workUnits;
        }

        foreach (var unit in _messages.UnitCollection.Where(x => x.Slot == slotId))
        {
            workUnits.Add(await BuildWorkUnit(slotId, slotDescription, unit).ConfigureAwait(false));
        }

        var currentId = GetCurrentWorkUnitId(slotId);
        if (currentId.HasValue)
        {
            workUnits.CurrentId = currentId.Value;
        }

        // if the previous unit has already left the UnitCollection then find the log section and update here
        if (PreviousWorkUnitShouldBeCompleted(workUnits, previousWorkUnit))
        {
            var unitRun = _messages.ClientRun.GetUnitRun(slotId, previousWorkUnit.Id, previousWorkUnit);
            if (unitRun is not null)
            {
                workUnits.Add(CompleteWorkUnitWithLogData(previousWorkUnit, unitRun));
            }
        }

        return workUnits;
    }

    private int? GetCurrentWorkUnitId(int slotId)
    {
        var units = _messages.UnitCollection!;

        foreach (var state in new[] { "RUNNING", "READY" })
        {
            int? currentId = units
                .FirstOrDefault(x => x.Slot == slotId && x.State.Equals(state, StringComparison.OrdinalIgnoreCase))
                ?.ID;
            if (currentId.HasValue)
            {
                return currentId;
            }
        }

        return null;
    }

    private static bool PreviousWorkUnitShouldBeCompleted(
        WorkUnitCollection workUnits,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] WorkUnit? previousWorkUnit)
    {
        return previousWorkUnit is not null &&
               !ContainsPreviousWorkUnitId() &&
               !ContainsPreviousWorkUnitByProjectAndAssignedTime();

        bool ContainsPreviousWorkUnitId()
        {
            return workUnits.ContainsId(previousWorkUnit.Id);
        }

        bool ContainsPreviousWorkUnitByProjectAndAssignedTime()
        {
            return workUnits.Any(EqualsProjectAndAssignedTime);
        }

        bool EqualsProjectAndAssignedTime(WorkUnit? other)
        {
            return other is not null &&
                   previousWorkUnit.HasProject() &&
                   other.HasProject() &&
                   previousWorkUnit.EqualsProject(other) &&
                   previousWorkUnit.Assigned.Equals(other.Assigned);
        }
    }

    private async Task<WorkUnit> BuildWorkUnit(int slotId, FahClientSlotDescription slotDescription, Unit unit)
    {
        var projectInfo = unit.ToProjectInfo();
        var unitRun = _messages.ClientRun.GetUnitRun(slotId, unit.ID.GetValueOrDefault(), projectInfo);

        var workUnitResult = WorkUnitResult.Parse(unitRun?.Data.WorkUnitResult);
        var protein = await _proteinService.GetOrRefresh(unit.Project.GetValueOrDefault()).ConfigureAwait(false) ?? new Protein();

        return new WorkUnit
        {
            Id = unit.ID.GetValueOrDefault(),
            UnitRetrievalTime = GetUnitRetrievalTime(),
            DonorName = _messages.Options?[Options.User] ?? Unknown.Value,
            DonorTeam = ToNullableInt32(_messages.Options?[Options.Team]).GetValueOrDefault(),
            Assigned = unit.AssignedDateTime.GetValueOrDefault(),
            Timeout = unit.TimeoutDateTime.GetValueOrDefault(),
            UnitStartTimeStamp = unitRun?.Data.UnitStartTimeStamp ?? TimeSpan.Zero,
            Finished = workUnitResult.IsTerminating ? DateTime.UtcNow : null,
            Core = unit.Core,
            CoreVersion = ParseCoreVersion(unitRun?.Data.CoreVersion),
            ProjectId = unit.Project.GetValueOrDefault(),
            ProjectRun = unit.Run.GetValueOrDefault(),
            ProjectClone = unit.Clone.GetValueOrDefault(),
            ProjectGen = unit.Gen.GetValueOrDefault(),
            UnitHex = unit.UnitHex,
            Protein = protein,
            Platform = BuildWorkUnitPlatform(slotDescription, unitRun),
            Result = workUnitResult,
            LogLines = unitRun is null ? null : LogLineEnumerable.Create(unitRun).ToList(),
            Frames = (IReadOnlyDictionary<int, LogLineFrameData>?)unitRun?.Data.Frames,
            FramesObserved = unitRun?.Data.FramesObserved ?? default
        };
    }

    private WorkUnit CompleteWorkUnitWithLogData(WorkUnit previousWorkUnit, UnitRun unitRun)
    {
        var workUnitResult = WorkUnitResult.Parse(unitRun.Data.WorkUnitResult);

        return previousWorkUnit with
        {
            UnitRetrievalTime = GetUnitRetrievalTime(),

            LogLines = LogLineEnumerable.Create(unitRun).ToList(),
            Frames = (IReadOnlyDictionary<int, LogLineFrameData>)unitRun.Data.Frames,
            UnitStartTimeStamp = unitRun.Data.UnitStartTimeStamp ?? TimeSpan.MinValue,
            FramesObserved = unitRun.Data.FramesObserved,
            CoreVersion = ParseCoreVersion(unitRun.Data.CoreVersion),
            Result = workUnitResult,
            Finished = workUnitResult.IsTerminating ? DateTime.UtcNow : null,
        };
    }

    private DateTime? _unitRetrievalTime;

    private DateTime GetUnitRetrievalTime() => _unitRetrievalTime ??= DateTime.UtcNow;

    private WorkUnitPlatform? BuildWorkUnitPlatform(FahClientSlotDescription slotDescription, UnitRun? unitRun)
    {
        string? implementation = ToPlatformImplementation(slotDescription.SlotType, unitRun);
        if (String.IsNullOrEmpty(implementation))
        {
            return null;
        }

        var systemInfo = _messages.Info?.System;
        if (systemInfo is not null && slotDescription is FahClientGpuSlotDescription gpu)
        {
            string? targetGpu = null;
            if (gpu.GpuBus.HasValue && gpu.GpuSlot.HasValue)
            {
                targetGpu = String.Format(CultureInfo.InvariantCulture, "Bus:{0} Slot:{1}", gpu.GpuBus, gpu.GpuSlot);
            }
            else if (gpu.GpuDevice.HasValue)
            {
                targetGpu = String.Format(CultureInfo.InvariantCulture, "Device:{0}", gpu.GpuDevice);
            }

            if (targetGpu is not null)
            {
                string? cudaDevice = systemInfo.GPUInfos.Values
                    .Select(x => x.CUDADevice)
                    .FirstOrDefault(x => x is not null && x.Contains(targetGpu, StringComparison.Ordinal));
                var cuda = FahClientGpuDeviceDescription.Parse(cudaDevice);
                string? openClDevice = systemInfo.GPUInfos.Values
                    .Select(x => x.OpenCLDevice)
                    .FirstOrDefault(x => x is not null && x.Contains(targetGpu, StringComparison.Ordinal));
                var openCl = FahClientGpuDeviceDescription.Parse(openClDevice);

                if (cuda is not null || openCl is not null)
                {
                    var platformIsCuda = implementation.Equals(WorkUnitPlatformImplementation.CUDA, StringComparison.Ordinal);

                    return new WorkUnitPlatform(implementation)
                    {
                        DriverVersion = openCl?.Driver,
                        ComputeVersion = platformIsCuda ? cuda?.Compute : openCl?.Compute,
                        CUDAVersion = platformIsCuda ? cuda?.Driver : null
                    };
                }
            }
        }

        return new WorkUnitPlatform(implementation);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static string? ToPlatformImplementation(FahClientSlotType slotType, UnitRun? unitRun) =>
        slotType switch
        {
            FahClientSlotType.Unknown => null,
            FahClientSlotType.Cpu => WorkUnitPlatformImplementation.CPU,
            FahClientSlotType.Gpu => unitRun?.Data.Platform,
            _ => null,
        };

    private static Version? ParseCoreVersion(string? value) =>
        value is null
            ? null
            : Version.TryParse(value, out var version)
                ? version
                : null;

    private static int? ToNullableInt32(string? value) =>
        Int32.TryParse(value, out var result)
            ? result
            : null;
}

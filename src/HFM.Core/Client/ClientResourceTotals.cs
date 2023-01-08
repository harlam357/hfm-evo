using System.Runtime.Serialization;

using HFM.Core.WorkUnits;

namespace HFM.Core.Client;

[DataContract(Namespace = "")]
public readonly record struct ClientResourceTotals
{
    // ReSharper disable InconsistentNaming

    [DataMember]
    public double PPD { get; init; }

    [DataMember]
    public double UPD { get; init; }

    // ReSharper restore InconsistentNaming

    [DataMember]
    public int TotalSlots { get; init; }

    [DataMember]
    public int WorkingSlots { get; init; }

    public int NonWorkingSlots => TotalSlots - WorkingSlots;

    [DataMember]
    public CompletedAndFailedWorkUnits CompletedAndFailedWorkUnits { get; init; }

    /// <summary>
    /// Gets the aggregate production values for all client resources.
    /// </summary>
    public static ClientResourceTotals Sum(ICollection<ClientResource>? collection)
    {
        if (collection is null)
        {
            return new();
        }

        double ppd = 0.0;
        double upd = 0.0;
        var workUnits = default(CompletedAndFailedWorkUnits);
        int workingSlots = 0;

        foreach (var resource in collection)
        {
            ppd += resource.PointsPerDay;
            upd += resource.UnitsPerDay;
            workUnits += resource.CompletedAndFailedWorkUnits;

            if (resource.Status.IsRunning())
            {
                workingSlots++;
            }
        }

        return new()
        {
            PPD = ppd,
            UPD = upd,
            TotalSlots = collection.Count,
            WorkingSlots = workingSlots,
            CompletedAndFailedWorkUnits = workUnits
        };
    }
}

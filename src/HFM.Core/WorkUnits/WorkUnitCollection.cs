using HFM.Core.Collections;

namespace HFM.Core.WorkUnits;

public class WorkUnitCollection : IdentityCollection<WorkUnit>
{
    public WorkUnitCollection()
    {

    }

    public WorkUnitCollection(IEnumerable<WorkUnit> workUnits)
    {
        foreach (var workUnit in workUnits)
        {
            Add(workUnit);
        }
    }
}

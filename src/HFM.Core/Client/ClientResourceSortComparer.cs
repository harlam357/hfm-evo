using System.ComponentModel;

using HFM.Core.Collections;

namespace HFM.Core.Client;

public class ClientResourceSortComparer : SortComparer<ClientResource>
{
    public bool OfflineClientsLast { get; set; }

    public override bool SupportsAdvancedSorting => false;

    protected override int CompareInternal(ClientResource? x, ClientResource? y)
    {
        /* Get property values */
        var xValue = GetPropertyValue(x, Property);
        var yValue = GetPropertyValue(y, Property);
        var xStatusValue = x!.Status;
        var yStatusValue = y!.Status;
        var xNameValue = x.GetName();
        var yNameValue = y.GetName();

        // check for offline clients first
        if (OfflineClientsLast)
        {
            if (xStatusValue == ClientResourceStatus.Offline &&
                yStatusValue != ClientResourceStatus.Offline)
            {
                return 1;
            }
            if (yStatusValue == ClientResourceStatus.Offline &&
                xStatusValue != ClientResourceStatus.Offline)
            {
                return -1;
            }
        }

        int returnValue;

        /* Determine sort order */
        if (Direction == ListSortDirection.Ascending)
        {
            returnValue = CompareAscending(xValue, yValue);
        }
        else
        {
            returnValue = CompareDescending(xValue, yValue);
        }

        // if values are equal, sort via the client name (asc)
        if (returnValue == 0)
        {
            returnValue = CompareAscending(xNameValue, yNameValue);
        }

        return returnValue;
    }
}

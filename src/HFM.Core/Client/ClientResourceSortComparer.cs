using System.ComponentModel;

using HFM.Core.Collections;

namespace HFM.Core.Client;

public class ClientResourceSortComparer : SortComparer<ClientResource>
{
    public bool OfflineStatusLast { get; set; }

    public override bool SupportsAdvancedSorting => false;

    protected override int CompareInternal(ClientResource? x, ClientResource? y)
    {
        var xStatusValue = x!.Status;
        var yStatusValue = y!.Status;

        // check for offline resources first
        if (OfflineStatusLast)
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
        var xValue = GetPropertyValue(x, Property);
        var yValue = GetPropertyValue(y, Property);

        if (Direction == ListSortDirection.Ascending)
        {
            returnValue = CompareAscending(xValue, yValue);
        }
        else
        {
            returnValue = CompareDescending(xValue, yValue);
        }

        // if values are equal, sort via the name (asc)
        if (returnValue == 0)
        {
            var xNameValue = x.Name;
            var yNameValue = y.Name;
            returnValue = CompareAscending(xNameValue, yNameValue);
        }

        return returnValue;
    }
}

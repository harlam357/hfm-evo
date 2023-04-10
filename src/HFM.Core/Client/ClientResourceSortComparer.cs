using System.ComponentModel;

using HFM.Core.Collections;

namespace HFM.Core.Client;

public class ClientResourceSortComparer : SortComparer<ClientResource>
{
    public bool OfflineStatusLast { get; set; }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        var xValue = GetPropertyValue(x, Property);
        var yValue = GetPropertyValue(y, Property);

        int returnValue = Direction == ListSortDirection.Ascending
            ? CompareAscending(xValue, yValue)
            : CompareDescending(xValue, yValue);

        // if values are equal, sort via the name (asc)
        if (returnValue == 0)
        {
            returnValue = CompareAscending(x.Name, y.Name);
        }

        return returnValue;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static string DefaultSortPropertyName => nameof(ClientResource.Name);

    public static PropertyDescriptor GetSortPropertyOrDefault(string propertyName)
    {
        var properties = TypeDescriptor.GetProperties(typeof(ClientResource));
        return properties.Find(propertyName, true) ?? properties.Find(DefaultSortPropertyName, true)!;
    }
}

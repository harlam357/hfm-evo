using System.Runtime.Serialization;

using HFM.Core.Client;

namespace HFM.Core.Artifacts.SlotXml;

[DataContract(Namespace = "")]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class SlotSummary
{
    [DataMember(Order = 1)]
    public string? HfmVersion { get; set; }

    [DataMember(Order = 2)]
    public string? NumberFormat { get; set; }

    [DataMember(Order = 3)]
    public DateTime UpdateDateTime { get; set; }

    [DataMember(Order = 4)]
    public ClientResourceTotals? SlotTotals { get; set; }

#pragma warning disable CA1002 // Do not expose generic lists
    [DataMember(Order = 5)]
    public List<SlotData>? Slots { get; set; }
#pragma warning restore CA1002 // Do not expose generic lists
}

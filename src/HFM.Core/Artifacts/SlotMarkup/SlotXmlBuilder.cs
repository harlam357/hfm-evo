using System.ComponentModel;
using System.Drawing;

using AutoMapper;

using HFM.Core.Client;
using HFM.Core.Internal;
using HFM.Core.WorkUnits;
using HFM.Preferences;

namespace HFM.Core.Artifacts.SlotMarkup;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public readonly record struct SlotXmlBuilderResult(SlotSummary SlotSummary, ICollection<SlotDetail> SlotDetails);

public class SlotXmlBuilder
{
    private readonly IPreferences _preferences;
    private readonly IMapper _mapper;

    public SlotXmlBuilder(IPreferences preferences)
    {
        _preferences = preferences;
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SlotXmlBuilderProfile>()).CreateMapper();
    }

    public SlotXmlBuilderResult Build(ICollection<ClientResource> collection)
    {
        var updateDateTime = DateTime.Now;
        var slotSummary = CreateSlotSummary(collection, updateDateTime);
        var slotDetails = SortSlots(collection).Select(x => CreateSlotDetail(x, updateDateTime)).ToList();
        return new(slotSummary, slotDetails);
    }

    private SlotSummary CreateSlotSummary(ICollection<ClientResource> collection, DateTime updateDateTime) =>
        new()
        {
            HfmVersion = ApplicationVersion,
            NumberFormat = GetNumberFormat(DecimalPlaces),
            UpdateDateTime = updateDateTime,
            SlotTotals = ClientResourceTotals.Sum(collection),
            Slots = SortSlots(collection).Select(CreateSlotData).ToList()
        };

    private IEnumerable<ClientResource> SortSlots(IEnumerable<ClientResource> collection)
    {
        string sortColumn = FormSortColumn;
        var property = ClientResourceSortComparer.GetSortPropertyOrDefault(sortColumn);
        var direction = FormSortOrder;
        var sortComparer = new ClientResourceSortComparer { OfflineStatusLast = OfflineStatusLast };
        sortComparer.SetSortProperties(property, direction);
        return collection.OrderBy(x => x, sortComparer);
    }

    private SlotDetail CreateSlotDetail(ClientResource resource, DateTime updateDateTime) =>
        new()
        {
            HfmVersion = ApplicationVersion,
            NumberFormat = GetNumberFormat(DecimalPlaces),
            UpdateDateTime = updateDateTime,
            LogFileAvailable = LogFileAvailable,
            LogFileName = resource.LogFileName,
            TotalRunCompletedUnits = resource.CompletedAndFailedWorkUnits.RunCompleted,
            TotalCompletedUnits = resource.CompletedAndFailedWorkUnits.TotalCompleted,
            TotalRunFailedUnits = resource.CompletedAndFailedWorkUnits.RunFailed,
            TotalFailedUnits = resource.CompletedAndFailedWorkUnits.TotalFailed,
            SlotData = CreateSlotData(resource)
        };

    private SlotData CreateSlotData(ClientResource resource)
    {
        // to prevent large xml files, limit the number of log lines
        const int maxLogLines = 500;

        var status = resource.Status;

        var slotData = new SlotData();
        slotData.Status = status.ToUserString();
        slotData.StatusColor = ColorTranslator.ToHtml(status.GetStatusColor());
        slotData.StatusFontColor = ColorTranslator.ToHtml(GetHtmlFontColor(status));
        slotData.PercentComplete = resource.Progress;
        slotData.Name = resource.Name;
        slotData.SlotType = resource.ResourceType;
        slotData.Processor = resource.Processor;
        slotData.ClientVersion = resource.Platform?.ClientVersion;
        slotData.TPF = resource.FrameTime.ToString();
        slotData.PPD = resource.PointsPerDay;
        slotData.UPD = resource.UnitsPerDay;
        slotData.ETA = resource.ETA.ToLocalString();
        slotData.Core = resource.Core;
        slotData.CoreId = resource.FormatCore(false).Replace("0x", String.Empty).ToUpperInvariant();
        slotData.ProjectIsDuplicate = false;
        slotData.ProjectRunCloneGen = resource.WorkUnit?.ToShortProjectString();
        slotData.Credit = resource.Credit;
        // TODO: ClientResource Completed and Failed
        slotData.Completed = 0; //resource.Completed;
        slotData.Failed = 0; //resource.Failed;
        slotData.TotalRunCompletedUnits = resource.CompletedAndFailedWorkUnits.RunCompleted;
        slotData.TotalCompletedUnits = resource.CompletedAndFailedWorkUnits.TotalCompleted;
        slotData.TotalRunFailedUnits = resource.CompletedAndFailedWorkUnits.RunFailed;
        slotData.TotalFailedUnits = resource.CompletedAndFailedWorkUnits.TotalFailed;
        slotData.UsernameOk = true;
        slotData.Username = resource.FormatDonorIdentity();
        slotData.DownloadTime = resource.WorkUnit?.Assigned.ToLocalTime().ToShortStringOrEmpty();
        slotData.PreferredDeadline = resource.WorkUnit?.Timeout.ToLocalTime().ToShortStringOrEmpty();
        var logLines = resource.LogLines;
        slotData.CurrentLogLines = logLines?.Skip(logLines.Count - maxLogLines).Select(_mapper.Map<Log.LogLine, LogLine>).ToList();
        slotData.Protein = _mapper.Map<Proteins.Protein?, Protein>(resource.WorkUnit?.Protein);
        return slotData;
    }

#pragma warning disable IDE0072 // Add missing cases
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static Color GetHtmlFontColor(ClientResourceStatus status) =>
        status switch
        {
            ClientResourceStatus.RunningNoFrameTimes => Color.Black,
            ClientResourceStatus.Paused => Color.Black,
            ClientResourceStatus.Finishing => Color.Black,
            ClientResourceStatus.Offline => Color.Black,
            ClientResourceStatus.Disabled => Color.Black,
            _ => Color.White
        };
#pragma warning restore IDE0072 // Add missing cases

    private string ApplicationVersion => _preferences.Get<string>(Preference.ApplicationVersion)!;

    private string FormSortColumn => _preferences.Get<string>(Preference.FormSortColumn)!;

    private ListSortDirection FormSortOrder => _preferences.Get<ListSortDirection>(Preference.FormSortOrder);

    private bool OfflineStatusLast => _preferences.Get<bool>(Preference.OfflineLast);

    private bool LogFileAvailable => _preferences.Get<bool>(Preference.WebGenCopyFAHlog);

    private int DecimalPlaces => _preferences.Get<int>(Preference.DecimalPlaces);

    private const string XsltNumberFormat = "###,###,##0";

    private static string GetNumberFormat(int decimalPlaces) =>
        decimalPlaces <= 0
            ? XsltNumberFormat
            : String.Concat(XsltNumberFormat, ".", new(Enumerable.Repeat('0', decimalPlaces).ToArray()));
}

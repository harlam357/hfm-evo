using System.Globalization;
using System.Text;

using HFM.Core.Client;
using HFM.Core.WorkUnits;
using HFM.Log;
using HFM.Preferences;

namespace HFM.Console.ViewModels;

public class ClientResourceViewModel : IClientResourceView
{
    private readonly ClientResource _clientResource;
    private readonly IPreferences _preferences;

    public ClientResourceViewModel(ClientResource clientResource, IPreferences preferences)
    {
        _clientResource = clientResource;
        _preferences = preferences;
    }

    public ClientResourceStatus Status =>
        _clientResource.CalculateStatus(PpdCalculation);

    public int Progress =>
        _clientResource.CalculateProgress(PpdCalculation);

    public string? Name =>
        _clientResource.Name;

    public string ResourceType =>
        _clientResource.FormatResourceType(ShowVersions);

    public string Processor =>
        _clientResource.FormatProcessor(ShowVersions);

    public TimeSpan FrameTime =>
        _clientResource.CalculateFrameTime(PpdCalculation);

    public double PointsPerDay =>
        Math.Round(_clientResource.CalculatePointsPerDay(PpdCalculation, BonusCalculation), DecimalPlaces);

    public string ETA
    {
        get
        {
            var etaValue = _clientResource.CalculateEtaValue(PpdCalculation, EtaAsDate);
            return etaValue.EtaDate.HasValue
                ? etaValue.EtaDate == DateTime.MinValue
                    ? String.Empty
                    : etaValue.EtaDate.Value.ToLocalTime().ToString(CultureInfo.CurrentCulture)
                : etaValue.Eta.ToString();
        }
    }

    public string Core =>
        _clientResource.FormatCore(ShowVersions);

    public string ProjectRunCloneGen => _clientResource.ProjectRunCloneGen;

    public double Credit =>
        Math.Round(_clientResource.CalculateCredit(PpdCalculation, BonusCalculation), DecimalPlaces);

    public int Completed { get; set; }

    public int Failed { get; set; }

    public string DonorIdentity => _clientResource.DonorIdentity;

    public DateTime Assigned => _clientResource.WorkUnit?.Assigned.ToLocalTime() ?? default;

    public DateTime Timeout => _clientResource.WorkUnit?.Timeout.ToLocalTime() ?? default;

    public IReadOnlyCollection<LogLine>? LogLines => _clientResource.LogLines;

    protected PpdCalculation PpdCalculation => _preferences.Get<PpdCalculation>(Preference.PPDCalculation);

    protected BonusCalculation BonusCalculation
    {
        get
        {
            var bonusCalculation = _preferences.Get<BonusCalculation>(Preference.BonusCalculation);
            return _clientResource.NormalizeBonusCalculation(PpdCalculation, bonusCalculation);
        }
    }

    protected int DecimalPlaces => _preferences.Get<int>(Preference.DecimalPlaces);

    protected bool ShowVersions => _preferences.Get<bool>(Preference.DisplayVersions);

    protected bool EtaAsDate => _preferences.Get<bool>(Preference.DisplayEtaAsDate);

    public static string HeaderString()
    {
        const string delimiter = " | ";

        var sb = new StringBuilder();
        sb.Append(FormatFixedWidth("Status", 9));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("%", 4));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Name", 19));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Resource Type", 15));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Processor", 20));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("TPF", 5));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("PPD", 9));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("ETA", 8));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Core", 13));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Project (R, C, G)", 24));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Credit", 8));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth("Identity", 16));
        //sb.Append(delimiter);
        //sb.Append(FormatFixedWidth(Assigned.ToString(CultureInfo.CurrentCulture), 22));
        //sb.Append(delimiter);
        //sb.Append(FormatFixedWidth(Timeout.ToString(CultureInfo.CurrentCulture), 22));

        sb.AppendLine();
        sb.Append(BarString(9));
        sb.Append(delimiter);
        sb.Append(BarString(4));
        sb.Append(delimiter);
        sb.Append(BarString(19));
        sb.Append(delimiter);
        sb.Append(BarString(15));
        sb.Append(delimiter);
        sb.Append(BarString(20));
        sb.Append(delimiter);
        sb.Append(BarString(5));
        sb.Append(delimiter);
        sb.Append(BarString(9));
        sb.Append(delimiter);
        sb.Append(BarString(8));
        sb.Append(delimiter);
        sb.Append(BarString(13));
        sb.Append(delimiter);
        sb.Append(BarString(24));
        sb.Append(delimiter);
        sb.Append(BarString(8));
        sb.Append(delimiter);
        sb.Append(BarString(16));
        //sb.Append(delimiter);
        //sb.Append(BarString(22));
        //sb.Append(delimiter);
        //sb.Append(BarString(22));

        return sb.ToString();
    }

    public override string ToString()
    {
        const string delimiter = " | ";

        var sb = new StringBuilder();
        sb.Append(FormatFixedWidth(Status.ToString(), 9));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth($"{Progress.ToString(CultureInfo.InvariantCulture)}%", 4));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(Name, 19));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(ResourceType, 15));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(Processor, 20));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(FrameTime.ToString(@"mm\:ss", CultureInfo.CurrentCulture), 5));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(PointsPerDay.ToString(CultureInfo.CurrentCulture), 9));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(ETA.ToString(CultureInfo.CurrentCulture), 8));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(Core, 13));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(ProjectRunCloneGen, 24));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(Credit.ToString(CultureInfo.CurrentCulture), 8));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(DonorIdentity, 16));
        //sb.Append(delimiter);
        //sb.Append(FormatFixedWidth(Assigned.ToString(CultureInfo.CurrentCulture), 22));
        //sb.Append(delimiter);
        //sb.Append(FormatFixedWidth(Timeout.ToString(CultureInfo.CurrentCulture), 22));
        return sb.ToString();
    }

    private static string FormatFixedWidth(string? value, int width)
    {
        if (value is null)
        {
            return EmptyString(width);
        }

        if (value.Length > width)
        {
            return value[..width];
        }

        if (value.Length < width)
        {
            return String.Concat(value, EmptyString(width - value.Length));
        }

        return value;
    }

    private static string EmptyString(int length) => new(Enumerable.Repeat(' ', length).ToArray());

    private static string BarString(int length) => new(Enumerable.Repeat('-', length).ToArray());
}

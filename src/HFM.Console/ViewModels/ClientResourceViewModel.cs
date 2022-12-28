using System.Globalization;
using System.Text;

using HFM.Core.Client;
using HFM.Core.WorkUnits;
using HFM.Log;
using HFM.Preferences;

namespace HFM.Console.ViewModels;

public abstract class ClientResourceViewModel
{
    private readonly ClientResource _clientResource;
    private readonly IPreferences _preferences;

    protected ClientResourceViewModel(ClientResource clientResource, IPreferences preferences)
    {
        _clientResource = clientResource;
        _preferences = preferences;
    }

    public static ClientResourceViewModel Create(ClientResource clientResource, IPreferences preferences)
    {
        if (clientResource is FahClientResource r)
        {
            return new FahClientResourceViewModel(r, preferences);
        }

        throw new ArgumentException("client resource type is invalid.", nameof(clientResource));
    }

    public virtual ClientResourceStatus Status =>
        _clientResource.Status == ClientResourceStatus.Running
            ? IsUsingBenchmarkFrameTime
                ? ClientResourceStatus.RunningNoFrameTimes
                : ClientResourceStatus.Running
            : _clientResource.Status;

    public virtual int Progress =>
        Status.IsRunning() || Status == ClientResourceStatus.Paused
            ? _clientResource.Progress
            : 0;

    public virtual string? Name { get; set; }

    public abstract string ResourceType { get; }

    public abstract string Processor { get; }

    public virtual TimeSpan TPF =>
        Status.IsRunning()
            ? _clientResource.WorkUnit?.GetFrameTime(PpdCalculation) ?? TimeSpan.Zero
            : TimeSpan.Zero;

    public virtual double PPD =>
        Status.IsRunning()
            ? Math.Round(_clientResource.WorkUnit?.GetPpd(PpdCalculation, BonusCalculation) ?? 0.0, DecimalPlaces)
            : 0.0;

    public virtual string ETA
    {
        get
        {
            var eta = _clientResource.WorkUnit?.GetEta(PpdCalculation);
            return eta is null
                ? String.Empty
                : Status.IsRunning()
                    ? EtaAsDate
                        ? _clientResource.WorkUnit?.UnitRetrievalTime.Add(eta.Value).ToLocalTime().ToString(CultureInfo.CurrentCulture) ?? String.Empty
                        : eta.Value.ToString()
                    : String.Empty;
        }
    }

    public virtual string Core
    {
        get
        {
            var core = _clientResource.WorkUnit?.Protein?.Core;
            var coreVersion = _clientResource.WorkUnit?.CoreVersion;

            return (ShowVersions && coreVersion is not null
                ? String.Format(CultureInfo.InvariantCulture, "{0} ({1})", core, coreVersion)
                : core) ?? String.Empty;
        }
    }

    public virtual string ProjectRunCloneGen =>
        _clientResource.WorkUnit.ToShortProjectString();

    public virtual double Credit =>
        Status.IsRunning()
            ? Math.Round(_clientResource.WorkUnit?.GetCredit(PpdCalculation, BonusCalculation) ?? 0.0, DecimalPlaces)
            : _clientResource.WorkUnit?.Protein?.Credit ?? 0.0;

    public virtual int Completed { get; set; }

    public virtual int Failed { get; set; }

    public virtual string DonorIdentity =>
        String.IsNullOrWhiteSpace(_clientResource.WorkUnit?.DonorName)
            ? String.Empty
            : String.Format(CultureInfo.InvariantCulture, "{0} ({1})", _clientResource.WorkUnit.DonorName, _clientResource.WorkUnit.DonorTeam);

    public virtual DateTime Assigned => _clientResource.WorkUnit?.Assigned.ToLocalTime() ?? default;

    public virtual DateTime Timeout => _clientResource.WorkUnit?.Timeout.ToLocalTime() ?? default;

    public IReadOnlyCollection<LogLine>? LogLines => _clientResource.LogLines;

    private bool IsUsingBenchmarkFrameTime =>
        _clientResource.WorkUnit.HasProject() && _clientResource.WorkUnit.GetRawTime(PpdCalculation) == 0;

    protected PpdCalculation PpdCalculation => _preferences.Get<PpdCalculation>(Preference.PPDCalculation);

    protected BonusCalculation BonusCalculation
    {
        get
        {
            var bonusCalculation = _preferences.Get<BonusCalculation>(Preference.BonusCalculation);
            if (bonusCalculation != BonusCalculation.None)
            {
                bonusCalculation = Status == ClientResourceStatus.RunningNoFrameTimes
                    ? BonusCalculation.FrameTime
                    : bonusCalculation;
            }
            return bonusCalculation;
        }
    }

    protected int DecimalPlaces => _preferences.Get<int>(Preference.DecimalPlaces);

    protected bool ShowVersions => _preferences.Get<bool>(Preference.DisplayVersions);

    protected bool EtaAsDate => _preferences.Get<bool>(Preference.DisplayEtaAsDate);

    public override string ToString()
    {
        const string delimiter = " | ";

        var sb = new StringBuilder();
        sb.Append(FormatFixedWidth(Status.ToString(), 10));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth($"{Progress.ToString(CultureInfo.InvariantCulture)}%", 4));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(ResourceType, 15));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(Processor, 31));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(TPF.ToString(@"mm\:ss", CultureInfo.CurrentCulture), 5));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(PPD.ToString(CultureInfo.CurrentCulture), 9));
        sb.Append(delimiter);
        sb.Append(FormatFixedWidth(ETA, 8));
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
}

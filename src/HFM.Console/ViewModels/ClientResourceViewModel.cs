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
        _clientResource.CalculateStatus(PpdCalculation);

    public virtual int Progress =>
        _clientResource.CalculateProgress(PpdCalculation);

    public virtual string? Name { get; set; }

    public abstract string ResourceType { get; }

    public abstract string Processor { get; }

    public virtual TimeSpan TPF =>
        _clientResource.GetFrameTime(PpdCalculation);

    public virtual double PPD =>
        Math.Round(_clientResource.GetPpd(PpdCalculation, BonusCalculation), DecimalPlaces);

    public virtual ClientResourceEtaValue ETA
    {
        get
        {
            TimeSpan eta = _clientResource.GetEta(PpdCalculation);
            DateTime? etaDate = EtaAsDate ? _clientResource.GetEtaDate(PpdCalculation) : null;
            return new ClientResourceEtaValue(eta, etaDate);
        }
    }

    public virtual string Core =>
        _clientResource.GetCore(ShowVersions);

    public virtual string ProjectRunCloneGen =>
        _clientResource.WorkUnit?.ToShortProjectString() ?? String.Empty;

    public virtual double Credit =>
        Math.Round(_clientResource.GetCredit(PpdCalculation, BonusCalculation), DecimalPlaces);

    public virtual int Completed { get; set; }

    public virtual int Failed { get; set; }

    public virtual string DonorIdentity =>
        String.IsNullOrWhiteSpace(_clientResource.WorkUnit?.DonorName)
            ? String.Empty
            : String.Format(CultureInfo.InvariantCulture, "{0} ({1})", _clientResource.WorkUnit.DonorName, _clientResource.WorkUnit.DonorTeam);

    public virtual DateTime Assigned => _clientResource.WorkUnit?.Assigned.ToLocalTime() ?? default;

    public virtual DateTime Timeout => _clientResource.WorkUnit?.Timeout.ToLocalTime() ?? default;

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
}

using System.Globalization;

namespace HFM.Core.Client;

public readonly struct ClientResourceEtaValue : IEquatable<ClientResourceEtaValue>, IComparable<ClientResourceEtaValue>, IComparable
{
    public TimeSpan Eta { get; }

    public DateTime? EtaDate { get; }

    public ClientResourceEtaValue(TimeSpan eta, DateTime? etaDate)
    {
        ValidateEtaDate(etaDate);

        Eta = eta;
        EtaDate = etaDate;
    }

    private static void ValidateEtaDate(DateTime? etaDate)
    {
        if (etaDate is null || etaDate.Value == DateTime.MinValue)
        {
            return;
        }
        if (etaDate.Value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException($"{nameof(etaDate)} must be DateTimeKind.Utc", nameof(etaDate));
        }
    }

    public override string ToString() => ToString(CultureInfo.CurrentCulture);

    public string ToString(IFormatProvider? formatProvider) =>
        EtaDate.HasValue
            ? EtaDate.Value.ToString(formatProvider)
            : Eta.ToString();

    public bool Equals(ClientResourceEtaValue other) => Eta.Equals(other.Eta);

    public override bool Equals(object? obj) => obj is ClientResourceEtaValue other && Equals(other);

    public override int GetHashCode() => Eta.GetHashCode();

    public static bool operator ==(ClientResourceEtaValue left, ClientResourceEtaValue right) => left.Equals(right);

    public static bool operator !=(ClientResourceEtaValue left, ClientResourceEtaValue right) => !left.Equals(right);

    public int CompareTo(ClientResourceEtaValue other) => Eta.CompareTo(other.Eta);

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        return obj is ClientResourceEtaValue other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ClientResourceEtaValue)}");
    }

    public static bool operator <(ClientResourceEtaValue left, ClientResourceEtaValue right) => left.CompareTo(right) < 0;

    public static bool operator >(ClientResourceEtaValue left, ClientResourceEtaValue right) => left.CompareTo(right) > 0;

    public static bool operator <=(ClientResourceEtaValue left, ClientResourceEtaValue right) => left.CompareTo(right) <= 0;

    public static bool operator >=(ClientResourceEtaValue left, ClientResourceEtaValue right) => left.CompareTo(right) >= 0;
}

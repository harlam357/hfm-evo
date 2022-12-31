using System.Globalization;
using System.Text.RegularExpressions;

namespace HFM.Core.Client;

public readonly partial struct FahClientSlotIdentifier : IEquatable<FahClientSlotIdentifier>, IComparable<FahClientSlotIdentifier>, IComparable
{
    public const int NoSlotId = -1;

    //public static FahClientSlotIdentifier AllSlots => new(0, "All Slots");

    //public bool IsAllSlots() => Equals(AllSlots);

    //private FahClientSlotIdentifier(int ordinal, string name)
    //{
    //    Ordinal = ordinal;
    //    ClientIdentifier = new ClientIdentifier(name, null, ClientSettings.NoPort, Guid.Empty);
    //    SlotId = NoSlotId;
    //}

    public FahClientSlotIdentifier(ClientIdentifier clientIdentifier, int slotId)
    {
        Ordinal = Int32.MaxValue;
        ClientIdentifier = clientIdentifier;
        SlotId = slotId;
    }

    public int Ordinal { get; }

    public ClientIdentifier ClientIdentifier { get; }

    public int SlotId { get; }

    public bool HasSlotId => SlotId != NoSlotId;

    public string Name => AppendSlotId(ClientIdentifier.Name!, SlotId);

    private static string AppendSlotId(string name, int slotId) =>
        slotId >= 0 ? String.Format(CultureInfo.InvariantCulture, "{0} Slot {1:00}", name, slotId) : name;

    public override string ToString() =>
        String.IsNullOrWhiteSpace(ClientIdentifier.Server)
            ? Name
            : String.Format(CultureInfo.InvariantCulture, "{0} ({1})", Name, ClientIdentifier.ToConnectionString());

    public bool Equals(FahClientSlotIdentifier other) =>
        Ordinal == other.Ordinal && ClientIdentifier.Equals(other.ClientIdentifier) && SlotId == other.SlotId;

    public override bool Equals(object? obj) =>
        obj is FahClientSlotIdentifier other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Ordinal, ClientIdentifier, SlotId);

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator ==(FahClientSlotIdentifier left, FahClientSlotIdentifier right) => left.Equals(right);

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator !=(FahClientSlotIdentifier left, FahClientSlotIdentifier right) => !left.Equals(right);

    public int CompareTo(FahClientSlotIdentifier other)
    {
        var ordinalComparison = Ordinal.CompareTo(other.Ordinal);
        if (ordinalComparison != 0) return ordinalComparison;
        var clientComparison = ClientIdentifier.CompareTo(other.ClientIdentifier);
        if (clientComparison != 0) return clientComparison;
        return SlotId.CompareTo(other.SlotId);
    }

    public int CompareTo(object? obj) =>
        obj is null
            ? 1
            : obj is FahClientSlotIdentifier other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(FahClientSlotIdentifier)}");

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator <(FahClientSlotIdentifier left, FahClientSlotIdentifier right) => left.CompareTo(right) < 0;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator >(FahClientSlotIdentifier left, FahClientSlotIdentifier right) => left.CompareTo(right) > 0;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator <=(FahClientSlotIdentifier left, FahClientSlotIdentifier right) => left.CompareTo(right) <= 0;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator >=(FahClientSlotIdentifier left, FahClientSlotIdentifier right) => left.CompareTo(right) >= 0;

    public static FahClientSlotIdentifier FromConnectionString(string? name, string connectionString, Guid guid)
    {
        var match = name is null ? null : NameSlotRegex().Match(name);
        return match is { Success: true }
            ? new FahClientSlotIdentifier(ClientIdentifier.FromConnectionString(match.Groups["Name"].Value, connectionString, guid), Int32.Parse(match.Groups["Slot"].Value, CultureInfo.InvariantCulture))
            : new FahClientSlotIdentifier(ClientIdentifier.FromConnectionString(name, connectionString, guid), NoSlotId);
    }

    [GeneratedRegex("(?<Name>.+) Slot (?<Slot>\\d\\d)$", RegexOptions.ExplicitCapture)]
    private static partial Regex NameSlotRegex();
}

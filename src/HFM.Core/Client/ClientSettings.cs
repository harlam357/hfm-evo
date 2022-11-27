using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace HFM.Core.Client;

public enum ClientType
{
    FahClient
}

[DataContract(Namespace = "")]
public record ClientSettings
{
    public ClientIdentifier ClientIdentifier => new(Name, Server, Port, Guid);

    /// <summary>
    /// Gets or sets the client type.
    /// </summary>
    [DataMember(Order = 1)]
    public ClientType ClientType { get; init; }

    /// <summary>
    /// Gets or sets the client name.
    /// </summary>
    [DataMember(Order = 2)]
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the client host name or IP address.
    /// </summary>
    [DataMember(Order = 3)]
    public string? Server { get; init; }

    /// <summary>
    /// Gets or sets the client host port number.
    /// </summary>
    [DataMember(Order = 4)]
    public int Port { get; init; } = DefaultPort;

    /// <summary>
    /// Gets or sets the client host password.
    /// </summary>
    [DataMember(Order = 5)]
    public string? Password { get; init; }

    /// <summary>
    /// Gets or sets the client unique identifier.
    /// </summary>
    [DataMember(Order = 6)]
    public Guid Guid { get; init; }

    /// <summary>
    /// Gets or set a value that determines if a client connection will be disabled.
    /// </summary>
    [DataMember(Order = 7)]
    public bool Disabled { get; init; }

    private const string FahClientLogFileName = "log.txt";

    public string ClientLogFileName => String.Format(CultureInfo.InvariantCulture, "{0}-{1}", Name, FahClientLogFileName);

    public const int NoPort = 0;

    /// <summary>
    /// The default Folding@Home client port.
    /// </summary>
    public const int DefaultPort = 36330;

    private const string NameFirstCharPattern = "[a-zA-Z0-9\\+=\\-_\\$&^\\[\\]]";
    private const string NameMiddleCharsPattern = "[a-zA-Z0-9\\+=\\-_\\$&^\\[\\] \\.]";
    private const string NameLastCharPattern = "[a-zA-Z0-9\\+=\\-_\\$&^\\[\\]]";

    /// <summary>
    /// Validates the client settings name.
    /// </summary>
    public static bool ValidateName(string? name)
    {
        if (name == null) return false;

        string pattern = String.Format(CultureInfo.InvariantCulture,
            "^{0}{1}+{2}$", NameFirstCharPattern, NameMiddleCharsPattern, NameLastCharPattern);
        return Regex.IsMatch(name, pattern, RegexOptions.Singleline);
    }
}

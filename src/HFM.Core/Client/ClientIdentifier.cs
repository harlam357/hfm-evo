﻿using System.Globalization;
using System.Text.RegularExpressions;

using HFM.Core.Net;

namespace HFM.Core.Client;

public readonly partial struct ClientIdentifier : IEquatable<ClientIdentifier>, IComparable<ClientIdentifier>, IComparable
{
    public ClientIdentifier(string? name, string? server, int port, Guid guid)
    {
        Name = name;
        Server = server;
        Port = port;
        Guid = guid;
    }

    public string? Name { get; }

    public string? Server { get; }

    public int Port { get; }

    public Guid Guid { get; }

    public bool HasGuid => Guid != Guid.Empty;

    public override string? ToString()
    {
        if (String.IsNullOrWhiteSpace(Server)) return Name;
        return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", Name, ToConnectionString());
    }

    public bool Equals(ClientIdentifier other)
    {
        if (HasGuid || other.HasGuid)
        {
            return Guid.Equals(other.Guid);
        }
        return Name == other.Name && Server == other.Server && Port == other.Port;
    }

    public override bool Equals(object? obj) => obj is ClientIdentifier other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            if (HasGuid)
            {
                return Guid.GetHashCode();
            }
            var hashCode = Name != null ? Name.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (Server != null ? Server.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Port;
            return hashCode;
        }
    }

    public static bool operator ==(ClientIdentifier left, ClientIdentifier right) => left.Equals(right);

    public static bool operator !=(ClientIdentifier left, ClientIdentifier right) => !left.Equals(right);

    public int CompareTo(ClientIdentifier other)
    {
        if (HasGuid)
        {
            return other.HasGuid ? Guid.CompareTo(other.Guid) : -1;
        }
        if (other.HasGuid)
        {
            return 1;
        }

        var nameComparison = String.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0) return nameComparison;
        var serverComparison = String.Compare(Server, other.Server, StringComparison.Ordinal);
        if (serverComparison != 0) return serverComparison;
        return Port.CompareTo(other.Port);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        return obj is ClientIdentifier other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ClientIdentifier)}");
    }

    public static bool operator <(ClientIdentifier left, ClientIdentifier right) => left.CompareTo(right) < 0;

    public static bool operator >(ClientIdentifier left, ClientIdentifier right) => left.CompareTo(right) > 0;

    public static bool operator <=(ClientIdentifier left, ClientIdentifier right) => left.CompareTo(right) <= 0;

    public static bool operator >=(ClientIdentifier left, ClientIdentifier right) => left.CompareTo(right) >= 0;

    public string? ToConnectionString() =>
        TcpPort.Validate(Port)
            ? String.Format(CultureInfo.InvariantCulture, "{0}:{1}", Server, Port)
            : Server;

    [GeneratedRegex("(?<Server>.+)[-:](?<Port>\\d+)$", RegexOptions.ExplicitCapture)]
    private static partial Regex FromConnectionStringRegex();

    public static ClientIdentifier FromConnectionString(string name, string? connectionString, Guid guid)
    {
        var match = connectionString is null ? null : FromConnectionStringRegex().Match(connectionString);
        return match is { Success: true }
            ? new ClientIdentifier(name, match.Groups["Server"].Value, Convert.ToInt32(match.Groups["Port"].Value, CultureInfo.InvariantCulture), guid)
            : new ClientIdentifier(name, connectionString, ClientSettings.NoPort, guid);
    }
}

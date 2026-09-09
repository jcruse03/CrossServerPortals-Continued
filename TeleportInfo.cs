using System;
using System.Net;

namespace Lunarbin.Valheim.CrossServerPortals;

public readonly struct TeleportInfo
{
    public const ushort DefaultValheimPort = 2456;

    public string Address { get; }
    public ushort Port { get; }
    public PortalType Type { get; }
    public string SourceTag { get; }
    public string TargetTag { get; }

    public enum PortalType
    {
        World,
        Server
    }

    public TeleportInfo(string sourceTag, string address, PortalType type, ushort port = 0, string targetTag = "")
    {
        SourceTag = sourceTag;
        Address = address;
        Type = type;
        Port = port;
        TargetTag = targetTag;
    }

    public static bool PortalTagIsTeleportInfo(string portalTag) => ParsePortalTag(portalTag).HasValue;

    /// <summary>
    /// Parses SourceTag|host[:port]|TargetTag and SourceTag|world:World Name|TargetTag.
    /// IPv6 addresses containing a port must use bracket notation, for example [::1]:2456.
    /// </summary>
    public static TeleportInfo? ParsePortalTag(string portalTag)
    {
        if (string.IsNullOrWhiteSpace(portalTag)) return null;

        string[] parts = portalTag.Split('|');
        if (parts.Length is < 2 or > 3) return null;

        string sourceTag = parts[0].Trim();
        string destination = parts[1].Trim();
        string targetTag = parts.Length == 3 ? parts[2].Trim() : string.Empty;

        if (sourceTag.Length == 0 || destination.Length == 0) return null;

        const string worldPrefix = "world:";
        if (destination.StartsWith(worldPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string worldName = destination.Substring(worldPrefix.Length).Trim();
            return worldName.Length == 0
                ? null
                : new TeleportInfo(sourceTag, worldName, PortalType.World, targetTag: targetTag);
        }

        return TryParseServer(destination, out string host, out ushort port)
            ? new TeleportInfo(sourceTag, host, PortalType.Server, port, targetTag)
            : null;
    }

    private static bool TryParseServer(string destination, out string host, out ushort port)
    {
        host = string.Empty;
        port = DefaultValheimPort;

        if (destination.StartsWith("[", StringComparison.Ordinal))
        {
            int closingBracket = destination.IndexOf(']');
            if (closingBracket <= 1) return false;

            host = destination.Substring(1, closingBracket - 1);
            string suffix = destination.Substring(closingBracket + 1);
            if (suffix.Length > 0 && (!suffix.StartsWith(":", StringComparison.Ordinal)
                                      || !TryParsePort(suffix.Substring(1), out port)))
            {
                return false;
            }

            return IPAddress.TryParse(host, out _);
        }

        if (IPAddress.TryParse(destination, out _))
        {
            host = destination;
            return true;
        }

        int firstColon = destination.IndexOf(':');
        int lastColon = destination.LastIndexOf(':');
        if (firstColon >= 0)
        {
            if (firstColon != lastColon) return false; // IPv6 with a port must be bracketed.
            host = destination.Substring(0, lastColon).Trim();
            if (!TryParsePort(destination.Substring(lastColon + 1), out port)) return false;
        }
        else
        {
            host = destination;
        }

        return IsValidHost(host);
    }

    private static bool TryParsePort(string value, out ushort port) =>
        ushort.TryParse(value, out port) && port > 0;

    private static bool IsValidHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253) return false;
        return IPAddress.TryParse(host, out _) || Uri.CheckHostName(host) != UriHostNameType.Unknown;
    }
}

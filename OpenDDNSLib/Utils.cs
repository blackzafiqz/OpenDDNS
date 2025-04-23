using System.Net;
using System.Net.Sockets;

namespace OpenDDNSLib;

public static class Utils
{
    public static AddressFamily GetAddressFamily(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out var addressFamily))
            return addressFamily.AddressFamily;

        return AddressFamily.Unknown;
    }
}
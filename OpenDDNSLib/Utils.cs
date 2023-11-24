using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenDDNSLib
{
    public static class Utils
    {
        public static AddressFamily GetAddressFamily(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out var addressFamily))
                return addressFamily.AddressFamily;

            return AddressFamily.Unknown; 
        }
    }
}

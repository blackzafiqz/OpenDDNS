using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenDDNSLib.Driver.Provider
{
    public class Rfc2136 : IProvider
    {
        public Task UpdateRecord(string domainName, string subdomainName, IPAddress ipAddress)
        {
            throw new NotImplementedException();
        }
    }
}

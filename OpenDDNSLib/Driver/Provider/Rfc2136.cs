using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDDNSLib.Driver.Provider
{
    public class Rfc2136 : IProvider
    {
        public Task<bool> UpdateRecord(string domainName, string subdomainName, string ipAddress)
        {
            throw new NotImplementedException();
        }
    }
}

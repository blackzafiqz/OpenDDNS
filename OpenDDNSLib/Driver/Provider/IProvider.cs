using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDDNSLib.Driver.Provider
{
    public interface IProvider
    {
        public Task<bool> UpdateRecord(string domainName, string subdomainName, string ipAddress);
    }
}

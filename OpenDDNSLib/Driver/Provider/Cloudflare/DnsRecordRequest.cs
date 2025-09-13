using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDDNSLib.Driver.Provider.Cloudflare
{
    public record DnsRecordRequest(
        string Type,
        string Name,
        string Content,
        int Ttl,
        bool Proxied
    );
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace OpenDDNS.Model
{
    internal class Configuration
    {
        
        public string Provider { get; set; }
        public string Domain { get; set; }
        public List<string> SubDomain { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public List<string>? ExtraParameters { get; set; }
        public bool IPv6 { get; set; } = false;
        public bool IPv4 { get; set; } = true;
        public string IPV4Resolver { get; set; } = "https://api64.ipify.org";
        public string IPv6Resolver { get; set; } = "https://api64.ipify.org";
    }
}

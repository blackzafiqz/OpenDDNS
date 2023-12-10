using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace OpenDDNS.Model
{
    public class Configuration
    {
        public Configuration()
        {

        }
        public Configuration(string provider, string domain, List<string> subDomain, string? username, string? password, List<string>? extraParameters, bool pv6, bool pv4, string ipv4Resolver, string pv6Resolver)
        {
            Provider = provider;
            Domain = domain;
            SubDomain = subDomain;
            Username = username;
            Password = password;
            ExtraParameters = extraParameters;
            IPv6 = pv6;
            IPv4 = pv4;
            IPV4Resolver = ipv4Resolver;
            IPv6Resolver = pv6Resolver;
        }

        public string Provider { get; set; }
        public string Domain { get; set; }
        public List<string> SubDomain { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public List<string>? ExtraParameters { get; set; }
        public bool IPv6 { get; set; } = false;
        public bool IPv4 { get; set; } = true;
        public string IPV4Resolver { get; set; } = "https://api.ipify.org";
        public string IPv6Resolver { get; set; } = "https://api64.ipify.org";

    }
}

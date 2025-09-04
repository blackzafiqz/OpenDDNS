

namespace OpenDDNS.Model
{
    internal class Configuration
    {
        public required string Domain { get; set; }
        public required List<string> SubDomains { get; set; }
        public List<string> DnsServers { get; set; } = new(){"1.1.1.1","8.8.8.8"};
        public int Interval { get; set; } = 5;
        public bool Ipv6 { get; set; } = false;
        public bool Ipv4 { get; set; } = true;
        public string Ipv4Resolver { get; set; } = "https://api.ipify.org";
        public string Ipv6Resolver { get; set; } = "https://api64.ipify.org";
        public PowerDns? PowerDns { get; set; }

    }
}

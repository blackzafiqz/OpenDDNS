

namespace OpenDDNS.Model
{
    internal class Configuration
    {
        public string Domain { get; set; }
        public List<string> SubDomain { get; set; }

        public int Interval { get; set; } = 5;
        public bool Ipv6 { get; set; } = false;
        public bool Ipv4 { get; set; } = true;
        public string Ipv4Resolver { get; set; } = "https://api.ipify.org";
        public string Ipv6Resolver { get; set; } = "https://api64.ipify.org";
        public PowerDns? PowerDns { get; set; }

    }
}

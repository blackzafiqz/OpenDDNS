

using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace OpenDDNS.Model
{
    public class Configuration
    {
        private static readonly string _configurationFile = "config.yaml";
        public required string Domain { get; set; }
        public required List<string> SubDomains { get; set; }
        public int Ttl { get; set; } = 300;
        public List<string> DnsServers { get; set; } = new() { "1.1.1.1", "8.8.8.8" };
        public int Interval { get; set; } = 5;
        public bool Ipv6 { get; set; } = false;
        public bool Ipv4 { get; set; } = true;
        public string Ipv4Resolver { get; set; } = "https://api.ipify.org";
        public string Ipv6Resolver { get; set; } = "https://api64.ipify.org";
        public PowerDns? PowerDns { get; set; }
        public Rfc2136? Rfc2136 { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Information;


        public static Configuration Load()
        {
            if (!File.Exists(_configurationFile))
            {
                throw new FileNotFoundException($"Configuration file '{_configurationFile}' not found.");
            }
            var deserializer = new DeserializerBuilder()
                .WithCaseInsensitivePropertyMatching()
                .Build();

            return deserializer.Deserialize<Configuration>(File.ReadAllText(_configurationFile));
        }
    }
}

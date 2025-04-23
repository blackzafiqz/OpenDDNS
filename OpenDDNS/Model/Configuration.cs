namespace OpenDDNS.Model;

public class Configuration
{
    public Configuration()
    {
    }

    public Configuration(string provider, string domain, List<string> subDomain, string? username, string? password,
        List<string>? extraParameters, bool pv6, bool pv4, string ipv4Resolver, string ipv6Resolver,int interval)
    {
        Provider = provider;
        Domain = domain;
        SubDomain = subDomain;
        Username = username;
        Password = password;
        ExtraParameters = extraParameters;
        IPv6 = pv6;
        IPv4 = pv4;
        IPv4Resolver = ipv4Resolver;
        IPv6Resolver = ipv6Resolver;
        Interval = interval;
    }

    public string Provider { get; set; }
    public string Domain { get; set; }
    public List<string> SubDomain { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public List<string>? ExtraParameters { get; set; }
    public bool IPv6 { get; set; }
    public bool IPv4 { get; set; } = true;
    public string IPv4Resolver { get; set; } = "https://api.ipify.org";
    public string IPv6Resolver { get; set; } = "https://api64.ipify.org";
    public int Interval { get; set; } = 300;
}
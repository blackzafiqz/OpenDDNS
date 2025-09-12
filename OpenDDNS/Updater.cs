using DnsClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDNS.Model;
using OpenDDNSLib.Driver.Provider;
using OpenDDNSLib.Exception;
using System.Net;
using System.Net.Sockets;
using PowerDns = OpenDDNSLib.Driver.Provider.PowerDns.PowerDns;
using Rfc2136 = OpenDDNSLib.Driver.Provider.Rfc2136.Rfc2136;
namespace OpenDDNS
{
    public class Updater : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<IProvider> _providers;
        private readonly Configuration _config;
        private readonly ILogger _logger;

        public Updater(HttpClient httpClient, ILogger<Updater> logger, Configuration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;

            try
            {
                _providers = GetProviders(_config);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogCritical(e.Message);
            }

        }

        private List<IProvider> GetProviders(Configuration config)
        {
            var providers = new List<IProvider>();

            if (config.PowerDns is { } pdns)
            {
                providers.Add(new PowerDns(
                    _httpClient,
                    _config.Ttl,
                    pdns.EndPoint,
                    pdns.ApiKey,
                    pdns.ServerId
                ));
            }

            if (config.Rfc2136 is { } rfc2136)
            {
                providers.Add(new Rfc2136(
                    _config.Ttl,
                    rfc2136.Name,
                    rfc2136.Key,
                    rfc2136.Server,
                    rfc2136.Algorithm
                ));
            }

            if (providers.Count == 0)
            {
                throw new InvalidOperationException("No valid DNS providers configured in config.yaml");
            }

            return providers;
        }




        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IPAddress? ipv4Address = null;
                IPAddress? ipv6Address = null;
                if (_config.Ipv4)
                    ipv4Address = await GetIpv4Address();
                if (_config.Ipv6)
                    ipv6Address = await GetIpv6Address();
                foreach (var domain in _config.Domains)
                {


                    foreach (var subDomain in domain.SubDomains)
                    {
                        if (_config.Ipv4)
                        {
                            var currentIpAddresses = await ResolveDomain($"{subDomain}.{domain.Name}", QueryType.A);
                            if (ipv4Address == null)
                            {

                                _logger.LogError("Invalid IPv4 Address");

                            }
                            else if (!currentIpAddresses.Any(ipv4Address.Equals))
                            {
                                _logger.LogDebug("Current IPv4: {ipAddress}", ipv4Address.ToString());
                                await UpdateDomain(ipv4Address, domain.Name, subDomain);
                            }
                        }

                        if (_config.Ipv6)
                        {
                            var currentIpAddresses =
                                await ResolveDomain($"{subDomain}.{domain.Name}", QueryType.AAAA);

                            if (ipv6Address == null)
                            {
                                _logger.LogError("Invalid IPv6 Address");
                            }
                            else if (!currentIpAddresses.Any(ipv6Address.Equals))
                            {
                                _logger.LogDebug("Current IPv6: {ipAddress}", ipv6Address.ToString());
                                await UpdateDomain(ipv6Address, domain.Name, subDomain);
                            }
                        }
                    }
                }

                await Task.Delay(_config.Interval * 60000, stoppingToken);
            }

        }
        private async Task<List<IPAddress>> ResolveDomain(string domainName, QueryType queryType)
        {
            var dnsServers = _config.DnsServers.Select(ip => new IPEndPoint(IPAddress.Parse(ip), 53)).ToArray();
            var clientOptions = new LookupClientOptions(dnsServers);
            var client = new LookupClient(clientOptions);
            var result = await client.QueryAsync(domainName, queryType);

            return queryType == QueryType.A ? result.Answers.ARecords().Select(a => a.Address).ToList() : result.Answers.AaaaRecords().Select(a => a.Address).ToList();
        }

        private async Task UpdateDomain(IPAddress ipAddress, string domain, string subDomain)
        {
            try
            {
                foreach (var provider in _providers)
                {


                    await provider.UpdateRecord(domain, subDomain, ipAddress);
                    _logger.LogInformation(
                        "Updated {ipType} for {subDomain}.{domain} : {ipAddress}",
                        ipAddress.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6", subDomain,
                        domain, ipAddress.ToString());
                }
            }
            catch (UpdateException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError("Failed to update {ipType} for {subDomain}.{domain} : {ipAddress}",
                    ipAddress.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6",
                    subDomain, domain, ipAddress.ToString());
            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
            }
        }

        private async Task<string> GetIpAddress(string resolver)
        {

            try
            {
                var response = await _httpClient.GetAsync(resolver);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return "";
            }

        }

        private async Task<IPAddress?> GetIpv6Address()
        {
            var ipString = await GetIpAddress(_config.Ipv6Resolver);
            try
            {
                var ipAddress = IPAddress.Parse(ipString);
                _logger.LogDebug("Detected IPv6: {ipAddress}", ipAddress.ToString());
                return ipAddress;

            }
            catch (FormatException e)
            {
                _logger.LogError(e, "Failed to parse IPv6: {ipString}", ipString);
            }

            return null;
        }
        private async Task<IPAddress?> GetIpv4Address()
        {
            var ipString = await GetIpAddress(_config.Ipv4Resolver);
            try
            {
                var ipAddress = IPAddress.Parse(ipString);
                _logger.LogDebug("Detected IPv4: {ipAddress}", ipAddress.ToString());
                return ipAddress;

            }
            catch (FormatException e)
            {

                _logger.LogError(e, "Failed to parse IPv4: {ipString}", ipString);
            }

            return null;
        }

    }
}

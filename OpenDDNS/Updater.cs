using DnsClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDNS.Model;
using OpenDDNSLib.Driver.Provider;
using System.Net;
using System.Net.Sockets;
using YamlDotNet.Serialization;
using PowerDns = OpenDDNSLib.Driver.Provider.PowerDns.PowerDns;
namespace OpenDDNS
{
    public class Updater : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IProvider? _provider;
        private readonly Configuration _config;
        private readonly ILogger _logger;
        private const string _configurationFile = "config.yaml";
        public Updater(HttpClient httpClient, ILogger<Updater> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var deserializer = new DeserializerBuilder()
                .WithCaseInsensitivePropertyMatching()
                .Build();
            _config = deserializer.Deserialize<Configuration>(File.ReadAllText(_configurationFile));
            _provider = GetProvider(_config);

        }

        private IProvider? GetProvider(Configuration config)
        {
            if (config.PowerDns != null)
                return new PowerDns(_httpClient, config.PowerDns.EndPoint, config.PowerDns.ApiKey, config.PowerDns.ServerId);
            return null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_provider == null)
            {
                _logger.LogError("Invalid provider");
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                IPAddress? ipv4Address = null;
                IPAddress? ipv6Address = null;
                if (_config.Ipv4)
                    ipv4Address = await GetIpv4Address();
                if (_config.Ipv6)
                    ipv6Address = await GetIpv6Address();
                foreach (var subDomain in _config.SubDomains)
                {
                    if (_config.Ipv4)
                    {
                        var currentIpAddresses = await ResolveDomain($"{subDomain}.{_config.Domain}", QueryType.A);
                        if (ipv4Address == null)
                        {
                            _logger.LogError("Invalid IPv4 Address");

                        }
                        else if (!currentIpAddresses.Any(ipv4Address.Equals))
                            await UpdateDomain(ipv4Address, subDomain);
                    }

                    if (_config.Ipv6)
                    {
                        var currentIpAddresses = await ResolveDomain($"{subDomain}.{_config.Domain}", QueryType.AAAA);
                        _logger.LogInformation($"Current ip: {ipv6Address}");
                        if (ipv6Address == null)
                        {
                            _logger.LogError("Invalid IPv6 Address");
                        }
                        else if (!currentIpAddresses.Any(ipv6Address.Equals))
                            await UpdateDomain(ipv6Address, subDomain);
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

            if (queryType == QueryType.A)
                return result.Answers.ARecords().Select(a => a.Address).ToList();
            else
                return result.Answers.AaaaRecords().Select(a => a.Address).ToList();
        }

        private async Task UpdateDomain(IPAddress ipAddress, string subDomain)
        {

            var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipAddress);
            if (res)
                _logger.LogInformation(
                $"Updated {(ipAddress.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6")} for {subDomain}.{_config.Domain} : {ipAddress.ToString()}");
            else
                _logger.LogError($"Failed to update {(ipAddress.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6")} for {subDomain}.{_config.Domain} : {ipAddress.ToString()}");

        }
        private async Task<string> GetIpAddress(string resolver)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(resolver);
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
                _logger.LogInformation($"IPAddress{ipAddress}");
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
                _logger.LogInformation($"IPAddress{ipAddress}");
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

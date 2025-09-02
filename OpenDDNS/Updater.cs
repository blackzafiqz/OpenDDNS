using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDNS.Model;
using OpenDDNSLib.Driver.Provider;
using System.Net;
using System.Net.Sockets;
using YamlDotNet.Serialization;
using PowerDns = OpenDDNSLib.Driver.Provider.PowerDns;

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
                if (_config.Ipv4)
                {

                    var ipAddress = await GetIpv4Address();
                    if (ipAddress == null)
                    {
                        _logger.LogError("Invalid IPv4 Address");

                    }
                    else
                        await UpdateDomain(ipAddress);
                }
                if (_config.Ipv6)
                {

                    var ipAddress = await GetIpv6Address();
                    if (ipAddress == null)
                    {
                        _logger.LogError("Invalid IPv6 Address");

                    }
                    else
                        await UpdateDomain(ipAddress);
                }
                await Task.Delay(_config.Interval * 60000, stoppingToken);
            }

        }

        private async Task UpdateDomain(IPAddress ipAddress)
        {
            foreach (var subDomain in _config.SubDomain)
            {
                var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipAddress);
                if (res)
                    _logger.LogInformation(
                    $"Updated {(ipAddress.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6")} for {subDomain}.{_config.Domain} : {ipAddress.ToString()}");
                else
                    _logger.LogError($"Failed to update {(ipAddress.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6")} for {subDomain}.{_config.Domain} : {ipAddress.ToString()}");
            }
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
                return ipAddress;

            }
            catch (FormatException e)
            {
                _logger.LogError($"Failed to parse ipv6: {e}");
            }

            return null;
        }
        private async Task<IPAddress?> GetIpv4Address()
        {
            var ipString = await GetIpAddress(_config.Ipv4Resolver);
            try
            {
                var ipAddress = IPAddress.Parse(ipString);
                return ipAddress;

            }
            catch (FormatException e)
            {
                _logger.LogError($"Failed to parse ipv4: {e}");
            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenDDNS.Model;
using OpenDDNSLib.Driver.Provider;

using OpenDDNS.Model;
using OpenDDNSLib;
using YamlDotNet.Serialization;
using System.Net;

namespace OpenDDNS
{
    public class Updater : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IProvider? _provider;
        private readonly Configuration _config;
        private const string _configurationFile = "config.yaml";
        public Updater(HttpClient httpClient)
        {
            _httpClient = httpClient;

            var deserializer = new Deserializer();
            _config = deserializer.Deserialize<Configuration>(File.ReadAllText(_configurationFile));
            _provider = GetProvider(_config);

        }

        IProvider? GetProvider(Configuration config)
        {
            IProvider? provider = config.Provider switch
            {
                "rfc2136" => new Rfc2136(),
                "pdns" => new PowerDns(_httpClient, config.ExtraParameters[0], config.Password, config.ExtraParameters[1]),
                _ => null
            };

            return provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_provider == null)
            {
                Console.WriteLine("Invalid provider");
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                var ipv4Address = await GetIpAddress(_config.IPV4Resolver);
                var ipv6Address = await GetIpAddress(_config.IPv6Resolver);

                if (ipv4Address == "" && ipv6Address == "")
                {
                    Console.WriteLine("Failed to get IP address");
                }
                else
                {
                    var ipv4AddressValid = IPAddress.TryParse(ipv4Address, out IPAddress parsedIP) &&
                                           parsedIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
                    var ipv6AddressValid = IPAddress.TryParse(ipv6Address, out parsedIP) &&
                                           parsedIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
                    foreach (var subDomain in _config.SubDomain)
                    {
                        if (_config.IPv6 && ipv6AddressValid)
                        {
                            Console.WriteLine($"Updating IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}");

                            var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipv6Address,
                                RecordType.AAAA);

                            Console.WriteLine(res
                                ? $"Updated IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}"
                                : $"Failed to update IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}");
                        }

                        if (_config.IPv4 && ipv4AddressValid)
                        {
                            Console.WriteLine($"Updating IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}");
                            var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipv4Address,
                                RecordType.A);
                            Console.WriteLine(res
                                ? $"Updated IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}"
                                : $"Failed to update IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}");
                        }
                    }
                }

                await Task.Delay(60000, stoppingToken);
            }

        }

        private async Task<string> GetIpAddress(string resolver)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(resolver);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
            return await response.Content.ReadAsStringAsync();
        }
    }
}

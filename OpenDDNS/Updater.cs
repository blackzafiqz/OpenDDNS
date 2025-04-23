using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using OpenDDNS.Model;
using OpenDDNSLib;
using OpenDDNSLib.Driver.Provider;
using YamlDotNet.Serialization;

namespace OpenDDNS;

public class Updater : BackgroundService
{
    private const string ConfigurationFile = "config.yaml";
    private readonly Configuration _config;
    private readonly HttpClient _httpClient;
    private readonly IProvider? _provider;

    public Updater(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var deserializer = new Deserializer();
        _config = deserializer.Deserialize<Configuration>(File.ReadAllText(ConfigurationFile));
        _provider = GetProvider(_config);
    }

    private IProvider? GetProvider(Configuration config)
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
        if (_provider == null) Console.WriteLine("Invalid provider");


        while (!stoppingToken.IsCancellationRequested)
        {
            var ipv4Address = await GetIpAddress(_config.IPv4Resolver);
            var ipv6Address = await GetIpAddress(_config.IPv6Resolver);

            if (ipv4Address == "" && ipv6Address == "")
            {
                Console.WriteLine("Failed to get IP address");
            }
            else
            {
                var ipv4AddressValid = IPAddress.TryParse(ipv4Address, out var parsedIp) &&
                                       parsedIp.AddressFamily == AddressFamily.InterNetwork;
                var ipv6AddressValid = IPAddress.TryParse(ipv6Address, out parsedIp) &&
                                       parsedIp.AddressFamily == AddressFamily.InterNetworkV6;
                foreach (var subDomain in _config.SubDomain)
                {
                    var addressList = await Dns.GetHostAddressesAsync($"{subDomain}.{_config.Domain}",cancellationToken:CancellationToken.None);
                    if (_config.IPv6 && ipv6AddressValid)
                    {
                        if (addressList.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6)
                            .Any(x => x.ToString() == ipv6Address))
                        {
                            Console.WriteLine($"IPv6 address already up to date for {subDomain}.{_config.Domain} : {ipv6Address}");
                            continue;
                        }
                        Console.WriteLine($"Updating IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}");

                        var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipv6Address,
                            RecordType.AAAA);

                        Console.WriteLine(res
                            ? $"Updated IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}"
                            : $"Failed to update IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}");
                    }

                    if (_config.IPv4 && ipv4AddressValid)
                    {
                        if (addressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                            .Any(x => x.ToString() == ipv4Address))
                        {
                            Console.WriteLine($"IPv4 address already up to date for {subDomain}.{_config.Domain} : {ipv4Address}");
                            continue;
                        }
                        Console.WriteLine($"Updating IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}");
                        var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipv4Address,
                            RecordType.A);
                        Console.WriteLine(res
                            ? $"Updated IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}"
                            : $"Failed to update IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}");
                    }
                }
            }

            await Task.Delay(_config.Interval*1000, stoppingToken);
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
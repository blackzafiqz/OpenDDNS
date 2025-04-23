using System.Net;
using System.Net.Sockets;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<Updater> Logger;
    public Updater(HttpClient httpClient, ILogger<Updater> logger)
    {
        _httpClient = httpClient;
        Logger = logger;

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
        if (_provider == null) Logger.LogCritical("Invalid provider");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var ipv4Address = await GetIpAddress(_config.IPv4Resolver);
            var ipv6Address = await GetIpAddress(_config.IPv6Resolver);

            if (ipv4Address == "" && ipv6Address == "")
            {
                Logger.LogError("Failed to get IP address");
            }
            else
            {
                var ipv4AddressValid = IPAddress.TryParse(ipv4Address, out var parsedIp) &&
                                       parsedIp.AddressFamily == AddressFamily.InterNetwork;
                var ipv6AddressValid = IPAddress.TryParse(ipv6Address, out parsedIp) &&
                                       parsedIp.AddressFamily == AddressFamily.InterNetworkV6;
                foreach (var subDomain in _config.SubDomain)
                {
                    IPAddress[] dnsServers;
                    try
                    { 
                        dnsServers = _config.DnsServers.Select(IPAddress.Parse).ToArray();
                    }
                    catch (FormatException e)
                    {
                        Logger.LogError($"Invalid DNS server address: {e.Message}");
                        return;
                    }
                    var lookupClient = new LookupClient(dnsServers);
                    var queryResponseA = await lookupClient.QueryAsync($"{subDomain}.{_config.Domain}", QueryType.A,cancellationToken: CancellationToken.None);
                    var queryResponseAaaa = await lookupClient.QueryAsync($"{subDomain}.{_config.Domain}", QueryType.AAAA,cancellationToken: CancellationToken.None);
                    if (_config.IPv6 && ipv6AddressValid)
                    {
                        if (queryResponseAaaa.Answers.AaaaRecords()
                            .Any(x => x.Address.ToString() == ipv6Address))
                            Logger.LogInformation($"IPv6 address already up to date for {subDomain}.{_config.Domain} : {ipv6Address}");
                        
                        else
                        {
                            Logger.LogInformation($"Updating IPv6 for {subDomain}.{_config.Domain} : {queryResponseAaaa.Answers.AaaaRecords().First().Address.ToString()} -> {ipv6Address}");

                            var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipv6Address,
                                RecordType.AAAA);

                            Logger.LogInformation(res
                                ? $"Updated IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}"
                                : $"Failed to update IPv6 for {subDomain}.{_config.Domain} : {ipv6Address}");
                        }
                    }

                    if (_config.IPv4 && ipv4AddressValid)
                    {
                        if (queryResponseA.Answers.ARecords()
                            .Any(x => x.Address.ToString() == ipv4Address))
                            Logger.LogInformation($"IPv4 address already up to date for {subDomain}.{_config.Domain} : {ipv4Address}");

                        else
                        {
                            Logger.LogInformation($"Updating IPv4 for {subDomain}.{_config.Domain} : {queryResponseA.Answers.ARecords().First().Address.ToString()} {ipv4Address}");
                            var res = await _provider!.UpdateRecord(_config.Domain, subDomain, ipv4Address,
                                RecordType.A);
                            Logger.LogInformation(res
                                ? $"Updated IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}"
                                : $"Failed to update IPv4 for {subDomain}.{_config.Domain} : {ipv4Address}");
                        }
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
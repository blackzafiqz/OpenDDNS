using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerDnsApi;
using YamlDotNet.Serialization;
namespace OpenDDNSLib.Driver.Provider
{
    public class PowerDns : IProvider
    {
        private readonly PowerDnsApi.PowerDnsApi _api;
        private readonly string _server;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public PowerDns(HttpClient httpClient, string baseUrl, string apiKey, string server)
        {
            _httpClient = httpClient;
            _api = new PowerDnsApi.PowerDnsApi(_httpClient);
            _api.BaseUrl = baseUrl;
            _server = server;
            _apiKey = apiKey;

        }
        public async Task<bool> UpdateRecord(string domainName, string subdomainName, string ipAddress, RecordType recordType)
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            try
            {
                Zone zone = new()
                {
                    Rrsets = new List<RRSet>()
                    {
                        new RRSet()
                        {
                            Name = $"{(subdomainName is not ("@" or "") ? subdomainName+"." : "")}{domainName}.",
                            Type = recordType == RecordType.A ? "A" : "AAAA",
                            Ttl = 300,
                            Changetype = "REPLACE",
                            Records =
                            {
                                new Record()
                                {
                                    Content = ipAddress,
                                    Disabled = false,
                                }
                            }
                        }
                    }
                };
                await _api.PatchZoneAsync(_server, domainName, zone);
            }
            catch (ApiException e)
            {
                Console.WriteLine(e);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
            }

            return true;
        }
    }
}

using PowerDnsApi;
using System.Net;
using System.Net.Sockets;
namespace OpenDDNSLib.Driver.Provider.PowerDns
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
            _api = new PowerDnsApi.PowerDnsApi(_httpClient)
            {
                BaseUrl = baseUrl
            };
            _server = server;
            _apiKey = apiKey;

        }
        public async Task<bool> UpdateRecord(string domainName, string subdomainName, IPAddress ipAddress)
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            try
            {
                Zone zone = new()
                {
                    Rrsets = new List<RRSet>
                    {
                        new()
                        {
                            Name = $"{(subdomainName is not ("@" or "") ? subdomainName+"." : "")}{domainName}.",
                            Type = ipAddress.AddressFamily == AddressFamily.InterNetwork ? "A" : "AAAA",
                            Ttl = 300,
                            Changetype = "REPLACE",
                            Records =
                            {
                                new Record
                                {
                                    Content = ipAddress.ToString(),
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

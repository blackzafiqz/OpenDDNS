using OpenDDNSLib.Exception;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace OpenDDNSLib.Driver.Provider.PowerDns
{
    public class PowerDns : IProvider
    {
        private readonly string _server;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public PowerDns(HttpClient httpClient, string baseUrl, string apiKey, string server)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl.Last() == '/' ? baseUrl : $"{baseUrl}/");
            _server = server;
            _apiKey = apiKey;

        }
        public async Task UpdateRecord(string domainName, string subdomainName, IPAddress ipAddress)
        {

            try
            {
                var rrSets = new List<RRSet>
                {
                    new()
                    {
                        Name = $"{(subdomainName is not ("@" or "") ? subdomainName + "." : "")}{domainName}.",
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
                };
                var payload = new { rrsets = rrSets };
                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("X-API-Key", _apiKey);
                var res = await _httpClient.PatchAsync($"servers/{_server}/zones/{domainName}.", content);
                res.EnsureSuccessStatusCode();

            }
            catch (HttpRequestException e)
            {
                throw new UpdateException(e.Message);
            }
        }
    }
}

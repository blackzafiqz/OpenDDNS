using OpenDDNSLib.Exception;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace OpenDDNSLib.Driver.Provider.Cloudflare
{
    public class Cloudflare : IProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly int _ttl;
        private readonly bool _proxied;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public Cloudflare(HttpClient httpClient, int ttl, string apiToken, bool proxied)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
            _apiToken = apiToken;
            _ttl = ttl;
            _proxied = proxied;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task UpdateRecord(string domainName, string subdomainName, IPAddress ipAddress)
        {
            try
            {
                var recordType = ipAddress.AddressFamily == AddressFamily.InterNetwork ? "A" : "AAAA";
                var fqdn = (subdomainName is not ("@" or ""))
                    ? $"{subdomainName}.{domainName}"
                    : domainName;

                var zone = await GetZoneByDomainName(domainName);

                var record = await GetDnsRecord(zone.Id, recordType, fqdn);

                var payload = new DnsRecordRequest(
                    recordType,
                    fqdn,
                    ipAddress.ToString(),
                    _ttl,
                    _proxied
                );

                var json = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage res;

                if (!string.IsNullOrEmpty(record?.Id))
                {
                    res = await _httpClient.PutAsync($"zones/{zone.Id}/dns_records/{record.Id}", content);
                }
                else
                {
                    res = await _httpClient.PostAsync($"zones/{zone.Id}/dns_records", content);
                }

                res.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                throw new UpdateException(e.Message);
            }
        }

        private async Task<DnsRecord?> GetDnsRecord(string zoneId, string recordType, string fqdn)
        {
            var recordReq = new HttpRequestMessage(HttpMethod.Get, $"zones/{zoneId}/dns_records?type={recordType}&name={fqdn}");

            var lookupResponse = await _httpClient.SendAsync(recordReq);
            lookupResponse.EnsureSuccessStatusCode();

            var lookup = await lookupResponse.Content.ReadFromJsonAsync<Response<DnsRecord>>(_jsonSerializerOptions);
            return lookup!.Result.FirstOrDefault();

        }
        private async Task<Zone> GetZoneByDomainName(string domainName)
        {
            var zoneResponse = await _httpClient.GetAsync($"zones?name={domainName}");
            zoneResponse.EnsureSuccessStatusCode();

            var zoneResult = await zoneResponse.Content.ReadFromJsonAsync<Response<Zone>>(_jsonSerializerOptions);

            if (zoneResult!.Result.Count == 0)
                throw new UpdateException($"Zone not found for domain {domainName}");

            return zoneResult.Result.First();
        }
    }

}

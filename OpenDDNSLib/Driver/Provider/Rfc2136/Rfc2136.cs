using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net.Dns.DynamicUpdate;
using OpenDDNSLib.Exception;
using System.Net;
using System.Net.Sockets;
using static ARSoft.Tools.Net.Dns.TSigAlgorithm;

namespace OpenDDNSLib.Driver.Provider.Rfc2136
{
    public class Rfc2136 : IProvider
    {
        private readonly string _name;
        private readonly byte[] _key;
        private readonly IPAddress _dnsServer;
        private readonly int _ttl;
        private readonly TSigAlgorithm _algorithm;
        public Rfc2136(int ttl, string name, string key, string dnsServer, string algorithm)
        {
            _name = name;
            _key = Convert.FromBase64String(key);
            _dnsServer = IPAddress.Parse(dnsServer);
            _ttl = ttl;

            if (!Enum.TryParse(algorithm, true, out _algorithm))
                throw new ArgumentException("Invalid A");

        }
        public async Task UpdateRecord(string domainName, string subdomainName, IPAddress ipAddress)
        {

            var domainUpdate = DomainName.Parse($"{(subdomainName is not ("@" or "") ? subdomainName + "." : "")}{domainName}");

            var msg = new DnsUpdateMessage()
            {
                ZoneName = DomainName.Parse(domainName)
            };

            msg.Updates.Add(new DeleteAllRecordsUpdate(domainUpdate, ipAddress.AddressFamily == AddressFamily.InterNetwork ?
                ARSoft.Tools.Net.Dns.RecordType.A : ARSoft.Tools.Net.Dns.RecordType.Aaaa));
            msg.Updates.Add(new AddRecordUpdate(ipAddress.AddressFamily == AddressFamily.InterNetwork ?
                new ARecord(domainUpdate, _ttl, ipAddress) : new AaaaRecord(domainUpdate, _ttl, ipAddress)));

            msg.TSigOptions = new TSigRecord(DomainName.Parse(_name), Sha512, DateTime.Now, new TimeSpan(0, 5, 0), msg.TransactionID,
                ReturnCode.NoError, null, _key);

            var dnsResult = await new DnsClient(_dnsServer, 5000).SendUpdateAsync(msg);
            if (dnsResult!.ReturnCode != ReturnCode.NoError)
            {
                throw new UpdateException($"DNS Server returned: {dnsResult.ReturnCode.ToString()}");
            }
        }
    }
}

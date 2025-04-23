namespace OpenDDNSLib.Driver.Provider;

public interface IProvider
{
    public Task<bool> UpdateRecord(string domainName, string subdomainName, string ipAddress, RecordType recordType);
}
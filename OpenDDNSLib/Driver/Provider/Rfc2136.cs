﻿namespace OpenDDNSLib.Driver.Provider;

public class Rfc2136 : IProvider
{
    public Task<bool> UpdateRecord(string domainName, string subdomainName, string ipAddress, RecordType recordType)
    {
        throw new NotImplementedException();
    }
}
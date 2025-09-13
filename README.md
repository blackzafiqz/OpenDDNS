# OpenDDNS

## Getting Started
To configure this project, a config.yaml file you must have. A template, config.example.yaml, is provided. To start your own configuration, copy the example file, you must.

cp config.example.yaml config.yaml

General Configuration
The following sections, all providers must have.
```
# Required
# The domain to update, required it is.
Domains:
  - Name: example.com
    # The subdomains to update, a list of strings, it must be.
    SubDomains:
      - "@"  # root domain
      - "" # root domain
      - www

# Optional
# Time-to-live for DNS records, in seconds. Default is 300.
Ttl: 300
# List of DNS servers to use. Default are "1.1.1.1" and "8.8.8.8".
DnsServers:
  - 172.104.53.29
# Interval between updates, in minutes. Default is 5.
Interval: 5
# Enable update for IPv6, true or false. Default is false.
Ipv6: true
# Enable update for IPv4, true or false. Default is true.
Ipv4: true
# The URL to resolve the IPv4 address. Default is https://api.ipify.org
Ipv4Resolver: https://api.ipify.org
# The URL to resolve the IPv6 address. Default is "https://api64.ipify.org
Ipv6Resolver: https://api64.ipify.org
# Log level for output. Default is Information.
# Possible values are: Trace, Debug, Information, Warning, Error, Critical, None.
LogLevel: Information
```

## Providers


### PowerDNS
For PowerDNS, this section you must use. Your API key and endpoint details, here you will place.
```
PowerDns:
  ApiKey: secretKey
  EndPoint: https://api.dns.example.com/api/v1/
  ServerId: localhost
```
### RFC2136
To use a provider with RFC2136, this section your key details will hold. Your key name, algorithm, and server, here they belong.
```
Rfc2136:
  Name: keyName
  Key: sdfkmlkklmasldkddcalskdcldddddklldl==
  # Possible values for algorithm are:
  # Md5, Sha1, Sha224, Sha256, Sha384, Sha512,
  # Sha256_128, Sha384_192, Sha512_256
  Algorithm: sha512
  Server: 172.104.53.29

```
### Cloudflare
```
Cloudflare:
    Apitoken: jksdavjn3kasdvjmnsjjhfhj3kk
    Proxied: false
```
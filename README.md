# OpenDDNS

# Configuration
## PowerDNS
```yaml
Provider: pdns
Domain: example.com
SubDomain:
  - www
Password: AStrongPassword
# Optional
ExtraParameters:
  - https://api.dns.server/api/v1 # Put your api dns server here
  - localhost

IPv6: true
IPv4: true
# Optional
# Defaulted to 300 seconds
# Update interval in seconds
Interval: 300
# Optional
# Defaulted to 1.1.1.1
# Dns servers use to resolve the domain
DnsServers:
  - 1.1.1.1
  - 2606:4700:4700::1111
```
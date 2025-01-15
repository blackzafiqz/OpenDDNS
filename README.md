# OpenDDNS

# Configuration
## PowerDNS
```yaml
Provider: pdns
Domain: example.com
SubDomain:
  - a
Password: AStrongPassword
ExtraParameters:
  - https://api.dns.server/api/v1 # Put your api dns server here
  - localhost
IPv6: true
IPv4: true
```
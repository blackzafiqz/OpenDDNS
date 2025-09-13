namespace OpenDDNS.Model
{
    public class Cloudflare
    {
        public required string ApiToken { get; set; }
        public bool Proxied { get; set; } = false;
    }
}

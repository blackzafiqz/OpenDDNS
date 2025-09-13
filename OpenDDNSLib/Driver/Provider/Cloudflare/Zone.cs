namespace OpenDDNSLib.Driver.Provider.Cloudflare
{
    public record Zone(
        string Id,
        string Name,
        string Status
    );
}

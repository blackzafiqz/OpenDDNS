namespace OpenDDNSLib.Driver.Provider.Cloudflare
{
    public record Response<T>(
        List<T> Result,
        bool Success,
        List<object> Errors,
        List<object> Messages
    );
}

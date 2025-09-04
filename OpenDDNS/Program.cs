using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenDDNS
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();
            builder.Services.AddHostedService<Updater>();
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDNS.Model;

namespace OpenDDNS
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var config = Configuration.Load();
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddLogging(logger =>
            {
                logger.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
                logger.AddFilter(x => x == config.LogLevel);
                logger.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                logger.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
                logger.AddFilter("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.None);
            });
            builder.Services.AddSingleton<Configuration>(config);
            builder.Services.AddHostedService<Updater>();
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
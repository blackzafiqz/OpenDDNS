using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenDDNS.Model;
using OpenDDNSLib.Driver.Provider;
using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace OpenDDNS
{
    internal class Program
    {
        
        static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddHostedService<Updater>();
            var host = builder.Build();
            await host.RunAsync();
        }
    }


}
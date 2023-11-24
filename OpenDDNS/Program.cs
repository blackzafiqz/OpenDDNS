using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenDDNS.Model;
using OpenDDNSLib.Driver.Provider;
using System;
using System.Net;
using System.Net.Sockets;
using YamlDotNet.Serialization;

namespace OpenDDNS
{
    internal class Program
    {
        static string configurationFile = "config.yaml";
        static async Task Main(string[] args)
        {
            var deserializer = new Deserializer();
            var config = deserializer.Deserialize<Configuration>(File.ReadAllText(configurationFile));
            
            HttpClient httpClient = new HttpClient();
            
        }
        static IProvider GetProvider(Configuration config)
        {
            IProvider provider=null;
            switch (config.Provider)
            {
                case "rfc2136":
                    provider = new Rfc2136();
                    break;
                case "pdns":
                    provider = new PowerDns();
                    break;
                
            }
            return provider;
        }
    }


}
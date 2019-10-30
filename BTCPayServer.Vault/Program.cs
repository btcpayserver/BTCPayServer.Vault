using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using BTCPayServer.Hwi;
using BTCPayServer.Hwi.Transports;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server;

namespace BTCPayServer.Vault
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(log => log.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost
                    .UseKestrel(kestrel =>
                    {
                        kestrel.ListenLocalhost(HttpTransport.LocalHwiDefaultPort);
                    })
                    .UseStartup<Startup>();
                })
                .Build();
            
            await host.StartAsync();
            var environment = host.Services.GetService<IWebHostEnvironment>();
            if (!environment.IsDevelopment())
            {
                var address = host.Services.GetService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.First();
                ProcessBrowser.Instance.OpenBrowser(address + "/Test.html");
            }
            await host.WaitForShutdownAsync();
        }
    }
}

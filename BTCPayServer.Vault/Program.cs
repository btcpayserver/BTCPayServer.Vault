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
using System.IO;

namespace BTCPayServer.Vault
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
#if RELOCATE_CONTENT
            hostBuilder.UseContentRoot(Path.GetDirectoryName(typeof(Program).Assembly.Location));
#endif
            using var host = hostBuilder
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
                var browser = host.Services.GetService<IBrowser>();
                var address = host.Services.GetService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.First();
                browser.OpenBrowser(address);
            }

            await host.WaitForShutdownAsync();
        }
    }
}

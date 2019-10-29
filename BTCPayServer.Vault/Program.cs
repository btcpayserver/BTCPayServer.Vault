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

namespace BTCPayServer.Vault
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHwiServer();
                })
                .ConfigureLogging(log => log.AddConsole())
                .Configure(app =>
                {
                    app.UseStaticFiles();
                    app.UseHwiServer();
                })
                .UseKestrel(kestrel =>
                {
                    kestrel.Listen(IPAddress.Loopback, 65092);
                })
                .Build();
            try
            {
                await host.StartAsync();
                var version = await new HwiClient(Network.Main)
                {
                    Transport = new HttpTransport()
                }.GetVersionAsync();
                var address = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First();
                Console.WriteLine("Listening on " + address);
                Console.ReadLine();
            }
            finally
            {
                await host.StopAsync();
            }
        }
    }
}

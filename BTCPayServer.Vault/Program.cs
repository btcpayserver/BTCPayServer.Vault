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
using Avalonia;
using Microsoft.AspNetCore.Connections;
using System.Net.Sockets;
using System.Threading;

namespace BTCPayServer.Vault
{
    class Program
    {
        //static SemaphoreSlim _
        public static void Main(string[] args)
        {
            if (!TestPortFree())
                return;
            var host = Host.CreateDefaultBuilder(args)
                            .ConfigureWebHostDefaults(webHost =>
                            {
                                webHost
                                .UseKestrel(kestrel =>
                                {
                                    kestrel.ListenLocalhost(HttpTransport.LocalHwiDefaultPort);
                                })
                                .UseStartup<Startup>();
                            })
                            .ConfigureLogging(l =>
                            {
                                l.SetMinimumLevel(LogLevel.Trace);
#if DEBUG
                                l.AddFilter(LoggerNames.HwiServer, LogLevel.Debug);
#endif
                            })
                            .Build();
            
            CurrentServiceProvider = host.Services;
            
            host.Services.GetRequiredService<AppBuilder>()
                         .With(host.Services)
                         .With(host)
                         .StartWithClassicDesktopLifetime(args);
        }

        public static IServiceProvider CurrentServiceProvider { get; private set; }

        private static bool TestPortFree()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, HttpTransport.LocalHwiDefaultPort);
            try
            {
                listener.Start();
                listener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Unused, but make the designer happy
        public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();
    }
}

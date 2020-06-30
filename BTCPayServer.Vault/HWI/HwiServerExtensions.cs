using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Hwi;
using BTCPayServer.Hwi.Deployment;
using BTCPayServer.Hwi.Transports;
using BTCPayServer.Vault;
using BTCPayServer.Vault.HWI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HwiServerExtensions
    {
        public static IServiceCollection AddHwiServer(this IServiceCollection services, Action<HwiServerOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.AddCors();
            services.AddSingleton(HwiVersions.v1_1_2);
            services.AddHostedService<HwiDownloadTask>();
            services.AddScoped<HwiServer>();
            services.AddSingleton<ITransport>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<HwiServerOptions>>();
                return new InternalTransport(new CliTransport(options.Value.HwiDeploymentDirectory)
                {
                    Logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerNames.HwiServerCli)
                });
            });
            services.AddSingleton<IRunningIndicator>(provider =>
            {
                return provider.GetRequiredService<ITransport>() as InternalTransport;
            });
            if (configure != null)
                services.Configure(configure);
            return services;
        }

        public static IApplicationBuilder UseHwiServer(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));
            applicationBuilder.Map(new PathString("/hwi-bridge/v1"), app =>
            {
                app.UseCors(policy => policy.AllowAnyOrigin().WithMethods("POST"));
                app.Run(async ctx =>
                {
                    await ctx.RequestServices.GetRequiredService<HwiServer>().Handle(ctx);
                });
            });
            return applicationBuilder;
        }
    }
}

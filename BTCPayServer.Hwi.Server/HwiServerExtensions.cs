using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Hwi;
using BTCPayServer.Hwi.Deployment;
using BTCPayServer.Hwi.Server;
using BTCPayServer.Hwi.Transports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HwiServerExtensions
    {
        public static IServiceCollection AddHwiServer(this IServiceCollection services, Action<HwiServerOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.AddCors();
            services.AddSingleton(HwiVersions.v1_0_3);
            services.AddHostedService<HwiDownloadTask>();
            services.AddSingleton<ITransport>(provider =>
            {
                return new CliTransport()
                {
                    Logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("BTCPayServer.Hwi.Server.Cli")
                };
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
                    var transport = ctx.RequestServices.GetRequiredService<ITransport>();
                    if (await TryExtractArguments(ctx.Request, ctx.RequestAborted) is string[] args)
                    {
                        var response = await transport.SendCommandAsync(args, ctx.RequestAborted);
                        ctx.Response.StatusCode = 200;
                        await ctx.Response.WriteAsync(response, ctx.RequestAborted);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 404;
                    }
                });
            });
            return applicationBuilder;
        }

        private static async Task<string[]> TryExtractArguments(HttpRequest request, CancellationToken cancellationToken)
        {
            var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("params", out var parameters) &&
                parameters.ValueKind is JsonValueKind.Array)
            {
                var len = parameters.GetArrayLength();
                if (len > 255)
                    return null;
                string[] result = new string[len];
                int i = 0;
                foreach (var array in parameters.EnumerateArray())
                {
                    if (array.ValueKind != JsonValueKind.String)
                        return null;
                    result[i++] = array.GetString();
                }
                return result;
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTCPayServer.Vault.HWI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Vault.NFC
{
    public static class NFCServerExtensions
    {
        public static IServiceCollection AddNFCServer(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddCors();
            services.AddSingleton<NFCServer>();
            return services;
        }
        public static IApplicationBuilder UseNFCServer(this IApplicationBuilder applicationBuilder)
        {
            ArgumentNullException.ThrowIfNull(applicationBuilder);
            applicationBuilder.Map(new PathString("/nfc-bridge/v1"), app =>
            {
                app.UseCors(policy => policy.AllowAnyOrigin().WithMethods("POST"));
                app.Run(async ctx =>
                {
                    await ctx.RequestServices.GetRequiredService<NFCServer>().Handle(ctx);
                });
            });
            return applicationBuilder;
        }
    }
}

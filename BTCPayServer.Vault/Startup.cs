using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NicolasDorier.RateLimits;

namespace BTCPayServer.Vault
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHwiServer();
            services.AddHttpContextAccessor();
            services.AddScoped<HWI.IPermissionPrompt, PermissionPrompt>();
            services.AddSingleton<IBrowser>(ProcessBrowser.Instance);
            services.AddSingleton<Prompts>();
            services.AddRateLimits();
            services.AddMvc();
        }
        public void Configure(IApplicationBuilder app, RateLimitService rateLimitService)
        {
            rateLimitService.SetZone($"zone={RateLimitZones.Prompt} rate=4r/m burst=3");
            app.UseStaticFiles();
            app.UseHwiServer();
            app.UseRouting();
            app.UseEndpoints(e =>
            {
                e.MapControllers();
            });
        }
    }
}

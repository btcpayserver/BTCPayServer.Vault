using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Hwi.Transports;
using Microsoft.AspNetCore.Http;
using NicolasDorier.RateLimits;
using Microsoft.Extensions.Logging;
using BTCPayServer.Vault.Services;

namespace BTCPayServer.Vault.HWI
{
    internal class HwiServer
    {
        static object ThrottleSingletonObject = new object();
        private readonly ITransport Transport;
        private readonly IPermissionPrompt _permissionPrompt;
        private readonly RateLimitService _rateLimitService;
        private readonly PermissionsService _permissionsService;
        private readonly ILogger _logger;

        public HwiServer(ITransport transport, 
                        IPermissionPrompt permissionPrompt, 
                        RateLimitService rateLimitService,
                        PermissionsService permissionsService,
                        ILoggerFactory loggerFactory)
        {
            Transport = transport;
            _permissionPrompt = permissionPrompt;
            _rateLimitService = rateLimitService;
            _permissionsService = permissionsService;
            _logger = loggerFactory.CreateLogger(LoggerNames.HwiServer);
        }

        internal async Task Handle(HttpContext ctx)
        {
            if (ctx.Request.Path.Value == "")
            {
                if (!(await TryExtractArguments(ctx.Request, ctx.RequestAborted) is string[] args))
                {
                    ctx.Response.StatusCode = 400;
                    return;
                }
                var response = await Transport.SendCommandAsync(args, ctx.RequestAborted);
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync(response, ctx.RequestAborted);
                return;
            }
            else if (ctx.Request.Path.StartsWithSegments("/request-permission"))
            {
                if (!ctx.Request.Headers.TryGetValue("Origin", out var origin))
                {
                    ctx.Response.StatusCode = 400;
                    return;
                }

                if (!await _rateLimitService.Throttle(RateLimitZones.Prompt, ThrottleSingletonObject, ctx.RequestAborted))
                {
                    ctx.Response.StatusCode = 429;
                    return;
                }
                if (await _permissionsService.IsGranted(origin))
                {
                    ctx.Response.StatusCode = 200;
                    return;
                }
                if (!await _permissionPrompt.AskPermission(origin, ctx.RequestAborted))
                {
                    _logger.LogInformation($"Permission to {origin} got denied");
                    ctx.Response.StatusCode = 401;
                    return;
                }
                _logger.LogInformation($"Permission to {origin} got granted");
                ctx.Response.StatusCode = 200;
                return;
            }
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

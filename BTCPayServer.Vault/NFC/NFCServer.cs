#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Logging;
using BTCPayServer.NTag424;
using BTCPayServer.NTag424.PCSC;
using BTCPayServer.Vault.HWI;
using BTCPayServer.Vault.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NicolasDorier.RateLimits;
using PCSC;

namespace BTCPayServer.Vault.NFC
{
    public class NFCServer
    {
        static object ThrottleSingletonObject = new object();
        private readonly RateLimitService _rateLimitService;
        private readonly PermissionsService _permissionsService;
        private readonly ILogger _logger;
        private readonly IPermissionPrompt _permissionPrompt;
        public NFCServer(IPermissionPrompt permissionPrompt,
            RateLimitService rateLimitService,
            PermissionsService permissionsService,
            ILoggerFactory loggerFactory)
        {
            _permissionPrompt = permissionPrompt;
            _rateLimitService = rateLimitService;
            _permissionsService = permissionsService;
            _logger = loggerFactory.CreateLogger(LoggerNames.NFCServer);
        }

        PCSCContext? PCSCContext;
        IAPDUTransport? ApduTransport;

        internal async Task Handle(HttpContext ctx)
        {
            if (!ctx.Request.Headers.TryGetValue("Origin", out var origin))
            {
                ctx.Response.StatusCode = 400;
                return;
            }
            var originReason = new OriginReason(origin, "nfc");

            if (ctx.Request.Path.Value == "" || ctx.Request.Path.Value == "/")
            {
                if (!await _permissionsService.IsGranted(originReason))
                {
                    ctx.Response.StatusCode = 401;
                    return;
                }
                var transport = ApduTransport;
                if (transport is null)
                {
                    ctx.Response.StatusCode = 409;
                    return;
                }
                var apdu = await TryExtractAPDU(ctx.Request, ctx.RequestAborted);
                if (apdu is null)
                {
                    ctx.Response.StatusCode = 400;
                    return;
                }
                var resp = await transport.SendAPDU(apdu, ctx.RequestAborted);
                JsonObject response = new JsonObject()
                {
                    ["data"] = resp.Data.ToHex(),
                    ["status"] = resp.sw1sw2
                };
                ctx.Response.StatusCode = 200;
                ctx.Response.Headers["Content-Type"] = "application/json";
                await ctx.Response.WriteAsync(response.ToJsonString(), ctx.RequestAborted);
            }
            if (ctx.Request.Path.StartsWithSegments("/wait-for-card"))
            {
                if (!await _permissionsService.IsGranted(originReason))
                {
                    ctx.Response.StatusCode = 401;
                    return;
                }
                PCSCContext?.Dispose();
                PCSCContext = await PCSCContext.WaitForCard(ctx.RequestAborted);
                ApduTransport = new PCSCAPDUTransport(PCSCContext.CardReader);
                _logger.LogInformation($"NFC card detected");
                ctx.Response.StatusCode = 200;
                return;
            }
            if (ctx.Request.Path.StartsWithSegments("/wait-for-disconnected"))
            {
                if (!await _permissionsService.IsGranted(originReason))
                {
                    ctx.Response.StatusCode = 401;
                    return;
                }
                if (PCSCContext is null)
                {
                    ctx.Response.StatusCode = 409;
                    return;
                }
                await PCSCContext.WaitForDisconnected(ctx.RequestAborted);
                PCSCContext.Dispose();
                PCSCContext = null;
                ApduTransport = null;
                
                _logger.LogInformation($"NFC card disconnected");
                ctx.Response.StatusCode = 200;
                return;
            }
            else if (ctx.Request.Path.StartsWithSegments("/request-permission"))
            {
                if (!await _rateLimitService.Throttle(RateLimitZones.Prompt, ThrottleSingletonObject, ctx.RequestAborted))
                {
                    ctx.Response.StatusCode = 429;
                    return;
                }
                
                if (await _permissionsService.IsGranted(originReason))
                {
                    ctx.Response.StatusCode = 200;
                    return;
                }
                if (!await _permissionPrompt.AskPermission(originReason, ctx.RequestAborted))
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

        private static async Task<byte[]?> TryExtractAPDU(HttpRequest request, CancellationToken cancellationToken)
        {
            var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("apdu", out var apdu) &&
                apdu.ValueKind == JsonValueKind.String)
            {
                try
                {
                    return apdu.GetString().HexToBytes();
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}

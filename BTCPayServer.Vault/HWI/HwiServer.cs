using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Hwi.Transports;
using Microsoft.AspNetCore.Http;

namespace BTCPayServer.Hwi.Server
{
    internal class HwiServer
    {
        private readonly ITransport Transport;

        public HwiServer(ITransport transport)
        {
            Transport = transport;
        }

        internal async Task Handle(HttpContext ctx)
        {
            if (await TryExtractArguments(ctx.Request, ctx.RequestAborted) is string[] args)
            {
                var response = await Transport.SendCommandAsync(args, ctx.RequestAborted);
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync(response, ctx.RequestAborted);
            }
            else
            {
                ctx.Response.StatusCode = 404;
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

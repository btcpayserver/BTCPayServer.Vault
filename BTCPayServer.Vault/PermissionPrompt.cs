using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Vault.HWI;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using BTCPayServer.Vault.Controllers;
using Microsoft.AspNetCore.Http;

namespace BTCPayServer.Vault
{
    public class PermissionPrompt : IPermissionPrompt
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly Prompts _prompts;
        private readonly IBrowser _browser;
        HttpContext httpContext;
        public PermissionPrompt(LinkGenerator linkGenerator,
                                IHttpContextAccessor httpContextAccessor,
                                Prompts prompts,
                                IBrowser browser)
        {
            _linkGenerator = linkGenerator;
            _prompts = prompts;
            _browser = browser;
            httpContext = httpContextAccessor.HttpContext;
        }
        public async Task<bool> AskPermission(string origin, CancellationToken cancellationToken)
        {
            var id = NBitcoin.RandomUtils.GetUInt32();
            var link = _linkGenerator.GetUriByAction(
                httpContext,
                nameof(PromptController.Authorize),
                "Prompt",
                new { id }, pathBase: "/");
            _prompts.CreatePrompt(id, origin);
            _browser.OpenBrowser(link);
            return await _prompts.WaitPrompt(id, cancellationToken);
        }
    }
}
